using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Api.Data;
using SchoolERP.Api.Models;

namespace SchoolERP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TCController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TCController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Principal")]
        public async Task<ActionResult<IEnumerable<TransferCertificate>>> GetTransferCertificates()
        {
            return await _context.TransferCertificates
                .Include(tc => tc.Student)
                .OrderByDescending(tc => tc.AppliedDate)
                .ToListAsync();
        }

        [HttpPost("Apply")]
        [Authorize(Roles = "Student,Parent,Admin,Super Admin,School Admin")]
        public async Task<ActionResult<TransferCertificate>> ApplyTC([FromBody] TransferCertificate request)
        {
            var student = await _context.Students.FindAsync(request.StudentId);
            if (student == null) return NotFound("Student not found");
            
            // If parent or student is applying, verify they own the student record
            if (User.IsInRole("Parent") || User.IsInRole("Student"))
            {
                if (student.ParentContactNumber != User.Identity?.Name && student.Email != User.Identity?.Name)
                {
                    return Forbid();
                }
            }

            // Check if there is already a pending or approved TC for this student
            var existing = await _context.TransferCertificates
                .FirstOrDefaultAsync(t => t.StudentId == request.StudentId && (t.Status == "Pending" || t.Status == "Approved"));
            if (existing != null)
            {
                return BadRequest("A Transfer Certificate request already exists for this student.");
            }

            request.Status = "Pending";
            request.AppliedDate = SchoolERP.Api.Utils.TimeUtils.GetIstTime();
            request.CreatedDate = SchoolERP.Api.Utils.TimeUtils.GetIstTime();

            _context.TransferCertificates.Add(request);
            await _context.SaveChangesAsync();

            return Ok(request);
        }

        [HttpPut("{id}/Status")]
        [Authorize(Roles = "Admin,Super Admin,School Admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] TransferCertificate updateRequest)
        {
            var tc = await _context.TransferCertificates.Include(t => t.Student).FirstOrDefaultAsync(t => t.Id == id);
            if (tc == null) return NotFound();

            if (updateRequest.Status == "Approved")
            {
                // Generate TC Number
                var count = await _context.TransferCertificates.CountAsync(t => t.Status == "Approved");
                tc.TCNumber = $"TC{SchoolERP.Api.Utils.TimeUtils.GetIstTime().Year}{(count + 1):D4}";
                tc.IssueDate = SchoolERP.Api.Utils.TimeUtils.GetIstTime();
                
                // Set additional details from admin
                tc.AcademicProgress = updateRequest.AcademicProgress ?? tc.AcademicProgress;
                tc.Conduct = updateRequest.Conduct ?? tc.Conduct;

                // Mark student as inactive
                if (tc.Student != null)
                {
                    tc.Student.IsActive = false;
                }
            }

            tc.Status = updateRequest.Status;
            
            await _context.SaveChangesAsync();
            return Ok(tc);
        }

        [HttpPost("Generate")]
        [Authorize(Roles = "Admin,Super Admin,School Admin")]
        public async Task<ActionResult<TransferCertificate>> GenerateTC([FromBody] TransferCertificate request)
        {
            var student = await _context.Students.FindAsync(request.StudentId);
            if (student == null) return NotFound("Student not found");
            if (!student.IsActive) return BadRequest("Student is already inactive or has a TC issued.");

            // Generate TC Number
            var count = await _context.TransferCertificates.CountAsync();
            request.TCNumber = $"TC{SchoolERP.Api.Utils.TimeUtils.GetIstTime().Year}{(count + 1):D4}";
            request.IssueDate = SchoolERP.Api.Utils.TimeUtils.GetIstTime();
            request.CreatedDate = SchoolERP.Api.Utils.TimeUtils.GetIstTime();

            // Save TC
            _context.TransferCertificates.Add(request);

            // Hide student from active list
            student.IsActive = false;
            
            await _context.SaveChangesAsync();

            return Ok(request);
        }
    }
}
