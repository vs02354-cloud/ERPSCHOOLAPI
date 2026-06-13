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
    public class LeaveRequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LeaveRequestsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Principal,HR")]
        public async Task<ActionResult<IEnumerable<LeaveRequest>>> GetLeaveRequests()
        {
            return await _context.LeaveRequests
                .Include(l => l.Student)
                .OrderByDescending(l => l.StartDate)
                .ToListAsync();
        }

        [HttpGet("my-leaves")]
        public async Task<ActionResult<IEnumerable<LeaveRequest>>> GetMyLeaves()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue("UserType");
            var userName = User.Identity?.Name;

            if (userRole?.Equals("Parent", StringComparison.OrdinalIgnoreCase) == true)
            {
                var childrenIds = await _context.Students
                    .Where(s => s.ParentContactNumber == userName || s.ParentUserId == userId)
                    .Select(s => s.Id)
                    .ToListAsync();

                return await _context.LeaveRequests
                    .Include(l => l.Student)
                    .Where(l => l.StudentId != null && childrenIds.Contains(l.StudentId.Value))
                    .OrderByDescending(l => l.StartDate)
                    .ToListAsync();
            }
            else if (userRole?.Equals("Student", StringComparison.OrdinalIgnoreCase) == true)
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
                if (student == null) return Ok(new List<LeaveRequest>());

                return await _context.LeaveRequests
                    .Where(l => l.StudentId == student.Id)
                    .OrderByDescending(l => l.StartDate)
                    .ToListAsync();
            }
            else
            {
                // Employee leave is no longer supported in this table as EmployeeId was removed.
                return Ok(new List<LeaveRequest>());
            }
        }

        [HttpPost]
        public async Task<ActionResult<LeaveRequest>> CreateLeaveRequest(LeaveRequest leaveRequest)
        {
            leaveRequest.StartDate = SchoolERP.Api.Utils.TimeUtils.ConvertToIst(leaveRequest.StartDate);
            leaveRequest.EndDate = SchoolERP.Api.Utils.TimeUtils.ConvertToIst(leaveRequest.EndDate);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue("UserType");
            var userName = User.Identity?.Name;

            if (userRole?.Equals("Parent", StringComparison.OrdinalIgnoreCase) == true)
            {
                if (leaveRequest.StudentId == null || leaveRequest.StudentId == 0)
                    return BadRequest("Student ID is required for Parent applications.");

                var isValidChild = await _context.Students.AnyAsync(s => s.Id == leaveRequest.StudentId && (s.ParentContactNumber == userName || s.ParentUserId == userId));
                if (!isValidChild) return Forbid();
            }
            else if (userRole?.Equals("Student", StringComparison.OrdinalIgnoreCase) == true)
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
                if (student == null) return BadRequest("Student profile not found.");
                
                leaveRequest.StudentId = student.Id;
            }
            else
            {
                return BadRequest("Only Students and Parents can apply for leaves.");
            }

            leaveRequest.Status = "approve"; // As per requirement
            
            _context.LeaveRequests.Add(leaveRequest);
            await _context.SaveChangesAsync();

            // Auto-mark attendance as "On Leave" for the requested dates
            if (leaveRequest.StudentId.HasValue && leaveRequest.StudentId.Value > 0)
            {
                var studentId = leaveRequest.StudentId.Value;
                var currDate = leaveRequest.StartDate.Date;
                var endDate = leaveRequest.EndDate.Date;

                while (currDate <= endDate)
                {
                    var existingAttendance = await _context.Attendances
                        .FirstOrDefaultAsync(a => a.StudentId == studentId && a.Date.Date == currDate);

                    if (existingAttendance != null)
                    {
                        existingAttendance.Status = "On Leave";
                        existingAttendance.Remarks = "Leave Approved";
                    }
                    else
                    {
                        _context.Attendances.Add(new Attendance
                        {
                            StudentId = studentId,
                            Date = currDate,
                            Status = "On Leave",
                            Remarks = "Leave Approved"
                        });
                    }
                    currDate = currDate.AddDays(1);
                }
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetMyLeaves), new { id = leaveRequest.Id }, leaveRequest);
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelLeaveRequest(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue("UserType");
            var userName = User.Identity?.Name;

            var leaveRequest = await _context.LeaveRequests.FindAsync(id);
            if (leaveRequest == null) return NotFound();

            // Authorization
            if (userRole?.Equals("Parent", StringComparison.OrdinalIgnoreCase) == true)
            {
                var isValidChild = await _context.Students.AnyAsync(s => s.Id == leaveRequest.StudentId && (s.ParentContactNumber == userName || s.ParentUserId == userId));
                if (!isValidChild) return Forbid();
            }
            else if (userRole?.Equals("Student", StringComparison.OrdinalIgnoreCase) == true)
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
                if (student == null || leaveRequest.StudentId != student.Id) return Forbid();
            }
            else
            {
                return Forbid();
            }

            // Only allow cancellation if start date is today or in the future
            if (leaveRequest.StartDate.Date < SchoolERP.Api.Utils.TimeUtils.GetIstTime().Date)
            {
                return BadRequest("Cannot cancel past leave requests.");
            }

            if (leaveRequest.Status == "cancelled")
            {
                return BadRequest("Leave request is already cancelled.");
            }

            leaveRequest.Status = "cancelled";

            // Revert attendance records
            if (leaveRequest.StudentId.HasValue && leaveRequest.StudentId.Value > 0)
            {
                var studentId = leaveRequest.StudentId.Value;
                var currDate = leaveRequest.StartDate.Date;
                var endDate = leaveRequest.EndDate.Date;

                var attendancesToDelete = await _context.Attendances
                    .Where(a => a.StudentId == studentId && a.Date.Date >= currDate && a.Date.Date <= endDate && a.Status == "On Leave")
                    .ToListAsync();

                if (attendancesToDelete.Any())
                {
                    _context.Attendances.RemoveRange(attendancesToDelete);
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Leave request cancelled successfully" });
        }
        
        [HttpGet("my-students")]
        [Authorize(Roles = "Parent,PARENT,parent")]
        public async Task<ActionResult<IEnumerable<object>>> GetMyStudents()
        {
            var userName = User.Identity?.Name;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var students = await _context.Students
                .Where(s => s.ParentContactNumber == userName || s.ParentUserId == userId)
                .Select(s => new { s.Id, s.FirstName, s.LastName, s.CurrentClass })
                .ToListAsync();

            return Ok(students);
        }
    }
}
