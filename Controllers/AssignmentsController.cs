using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Api.Data;
using SchoolERP.Api.Models;
using System.Security.Claims;

namespace SchoolERP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AssignmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AssignmentsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // POST: api/Assignments
        [HttpPost]
        [Authorize(Roles = "Admin,ADMIN,admin,Principal,PRINCIPAL,principal,Super Admin,SUPER ADMIN,super admin,School Admin,SCHOOL ADMIN,school admin,Teacher,TEACHER,teacher")]
        public async Task<ActionResult<AssignmentMaster>> CreateAssignment(AssignmentMaster assignment)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                assignment.CreatedBy = userId;
            }
            assignment.CreatedDate = DateTime.UtcNow;

            _context.AssignmentMasters.Add(assignment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAssignment), new { id = assignment.AssignmentId }, assignment);
        }

        // GET: api/Assignments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AssignmentMaster>>> GetAssignments([FromQuery] string? className, [FromQuery] string? section)
        {
            var query = _context.AssignmentMasters.AsQueryable();

            if (!string.IsNullOrEmpty(className))
            {
                query = query.Where(a => a.ClassName == className);
            }
            if (!string.IsNullOrEmpty(section))
            {
                query = query.Where(a => a.Section == section);
            }

            return await query.OrderByDescending(a => a.CreatedDate).ToListAsync();
        }

        // GET: api/Assignments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetAssignment(int id)
        {
            var assignment = await _context.AssignmentMasters
                .Include(a => a.Submissions)
                .ThenInclude(s => s.Student)
                .FirstOrDefaultAsync(a => a.AssignmentId == id);

            if (assignment == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isStudent = User.FindFirstValue("UserType") == "Student";

            if (isStudent)
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
                if (student != null)
                {
                    // For student, only return their own submission
                    var studentSubmission = assignment.Submissions?.FirstOrDefault(s => s.StudentId == student.Id);
                    return new
                    {
                        Assignment = assignment,
                        MySubmission = studentSubmission
                    };
                }
            }

            // For teachers/admins, return assignment with all submissions
            return new
            {
                Assignment = assignment,
                AllSubmissions = assignment.Submissions
            };
        }

        // POST: api/Assignments/Upload
        [HttpPost("Upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Assignments");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = $"/Uploads/Assignments/{uniqueFileName}";
            return Ok(new { FilePath = relativePath });
        }

        // POST: api/Assignments/{id}/Submit
        [HttpPost("{id}/Submit")]
        [Authorize(Roles = "Student,STUDENT,student")]
        public async Task<IActionResult> SubmitAssignment(int id, [FromBody] AssignmentSubmission submission)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
            
            if (student == null) return Unauthorized("Student profile not found.");

            var assignment = await _context.AssignmentMasters.FindAsync(id);
            if (assignment == null) return NotFound("Assignment not found.");

            // Check if already submitted
            var existing = await _context.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.AssignmentId == id && s.StudentId == student.Id);

            if (existing != null)
            {
                return BadRequest("You have already submitted this assignment.");
            }

            submission.AssignmentId = id;
            submission.StudentId = student.Id;
            submission.SubmittedDate = DateTime.UtcNow;

            if (submission.SubmittedDate > assignment.DueDate)
            {
                submission.Status = "Late Submitted";
            }
            else
            {
                submission.Status = "Submitted";
            }

            _context.AssignmentSubmissions.Add(submission);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Assignment submitted successfully.", SubmissionId = submission.SubmissionId });
        }

        // PUT: api/Assignments/Submission/{id}/Evaluate
        [HttpPut("Submission/{id}/Evaluate")]
        [Authorize(Roles = "Admin,ADMIN,admin,Principal,PRINCIPAL,principal,Super Admin,SUPER ADMIN,super admin,School Admin,SCHOOL ADMIN,school admin,Teacher,TEACHER,teacher")]
        public async Task<IActionResult> EvaluateSubmission(int id, [FromBody] AssignmentSubmission evaluationData)
        {
            var submission = await _context.AssignmentSubmissions.FindAsync(id);
            if (submission == null) return NotFound();

            submission.MarksObtained = evaluationData.MarksObtained;
            submission.TeacherRemark = evaluationData.TeacherRemark;
            submission.Status = evaluationData.Status; // E.g., 'Evaluated', 'Rejected', 'Returned'

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Evaluation saved successfully." });
        }

        // GET: api/Assignments/Student
        [HttpGet("Student")]
        [Authorize(Roles = "Student,STUDENT,student")]
        public async Task<IActionResult> GetStudentAssignments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
            
            if (student == null) return Unauthorized("Student profile not found.");

            // Get assignments for the student's class and section
            var assignments = await _context.AssignmentMasters
                .Where(a => a.ClassName == student.CurrentClass && a.Section == student.Section)
                .Include(a => a.Submissions.Where(s => s.StudentId == student.Id))
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();

            var result = assignments.Select(a => new
            {
                a.AssignmentId,
                a.Title,
                a.Subject,
                a.DueDate,
                a.MaxMarks,
                MySubmission = a.Submissions?.FirstOrDefault()
            });

            return Ok(result);
        }
    }
}
