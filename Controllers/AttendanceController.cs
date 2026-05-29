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
    public class AttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("Mark")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Teacher,Principal")]
        public async Task<ActionResult<Attendance>> MarkAttendance(Attendance attendance)
        {
            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAttendance), new { id = attendance.Id }, attendance);
        }

        [HttpPost("MarkBulk")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Teacher,Principal")]
        public async Task<IActionResult> MarkBulkAttendance(List<Attendance> attendances)
        {
            foreach (var att in attendances)
            {
                var existing = await _context.Attendances
                    .FirstOrDefaultAsync(a => a.StudentId == att.StudentId && a.Date.Date == att.Date.Date);

                if (existing != null)
                {
                    existing.Status = att.Status;
                    existing.Remarks = att.Remarks;
                }
                else
                {
                    // Clear the Student object before saving to avoid tracking issues if it was passed from frontend
                    att.Student = null;
                    _context.Attendances.Add(att);
                }
            }
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Attendance marked successfully" });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Attendance>> GetAttendance(int id)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null) return NotFound();
            return attendance;
        }

        [HttpGet("Student/{studentId}")]
        public async Task<ActionResult<IEnumerable<Attendance>>> GetStudentAttendance(int studentId)
        {
            return await _context.Attendances
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        [HttpGet("ByDate/{date}")]
        public async Task<ActionResult<IEnumerable<Attendance>>> GetAttendanceByDate(DateTime date)
        {
            // Truncate time portion from the incoming date parameter
            var dateOnly = date.Date;

            return await _context.Attendances
                .Include(a => a.Student) // Include student to allow frontend to filter by class
                .Where(a => a.Date.Date == dateOnly)
                .ToListAsync();
        }
        [HttpGet("Report")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Principal,Teacher")]
        public async Task<ActionResult<object>> GetAttendanceReport(
            [FromQuery] DateTime date, 
            [FromQuery] string className, 
            [FromQuery] string section)
        {
            var dateOnly = date.Date;

            // Fetch all students in this class/section
            var students = await _context.Students
                .Where(s => s.CurrentClass == className && s.Section == section)
                .OrderBy(s => s.FirstName)
                .ToListAsync();

            if (!students.Any())
            {
                return Ok(new
                {
                    TotalStudents = 0,
                    Present = 0,
                    Absent = 0,
                    Late = 0,
                    AttendancePercentage = 0,
                    Records = new List<object>()
                });
            }

            // Fetch all attendances for these students on this date
            var studentIds = students.Select(s => s.Id).ToList();
            var attendances = await _context.Attendances
                .Where(a => a.Date.Date == dateOnly && studentIds.Contains(a.StudentId))
                .ToDictionaryAsync(a => a.StudentId);

            var records = new List<object>();
            int presentCount = 0;
            int absentCount = 0;
            int lateCount = 0;

            foreach (var student in students)
            {
                string status = "Not Marked";
                string remarks = "";

                if (attendances.TryGetValue(student.Id, out var attendance))
                {
                    status = attendance.Status;
                    remarks = attendance.Remarks ?? "";

                    if (status == "Present") presentCount++;
                    else if (status == "Absent") absentCount++;
                    else if (status == "Late") lateCount++;
                }

                records.Add(new
                {
                    StudentId = student.Id,
                    StudentName = $"{student.FirstName} {student.LastName}",
                    AdmissionNumber = student.AdmissionNumber,
                    Status = status,
                    Remarks = remarks
                });
            }

            int totalStudents = students.Count;
            // Attendance % based on Present + Late out of Total Students (or only Present, depends. usually Present + Late)
            int markedPresent = presentCount + lateCount;
            int attendancePercentage = totalStudents > 0 ? (int)Math.Round((double)markedPresent / totalStudents * 100) : 0;

            return Ok(new
            {
                TotalStudents = totalStudents,
                Present = presentCount,
                Absent = absentCount,
                Late = lateCount,
                AttendancePercentage = attendancePercentage,
                Records = records
            });
        }
    }
}
