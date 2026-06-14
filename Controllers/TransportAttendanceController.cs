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
    public class TransportAttendanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TransportAttendanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Transport Manager")]
        public async Task<ActionResult<IEnumerable<TransportAttendance>>> GetAttendances([FromQuery] DateTime? date)
        {
            var query = _context.TransportAttendances.Include(a => a.Student).AsQueryable();
            
            if (date.HasValue)
            {
                query = query.Where(a => a.Date.Date == date.Value.Date);
            }

            return await query.ToListAsync();
        }

        [HttpPost("mark")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Transport Manager,Driver,Attendant")]
        public async Task<ActionResult> MarkAttendance([FromBody] MarkTransportAttendanceDto dto)
        {
            var userName = User.Identity?.Name;
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Username == userName);
            
            var today = SchoolERP.Api.Utils.TimeUtils.GetIstTime().Date;
            var currentTime = SchoolERP.Api.Utils.TimeUtils.GetIstTime().TimeOfDay;

            var attendance = await _context.TransportAttendances
                .FirstOrDefaultAsync(a => a.StudentId == dto.StudentId && a.Date.Date == today);

            if (attendance == null)
            {
                attendance = new TransportAttendance
                {
                    StudentId = dto.StudentId,
                    Date = today,
                    ScannedByEmployeeId = employee?.Id,
                    Method = dto.Method
                };
                _context.TransportAttendances.Add(attendance);
            }

            if (dto.Type.ToLower() == "board")
            {
                attendance.BoardingTime = currentTime;
                attendance.Status = "Boarded";
            }
            else if (dto.Type.ToLower() == "deboard")
            {
                attendance.DeboardingTime = currentTime;
                attendance.Status = "Deboarded";
            }
            else
            {
                return BadRequest("Invalid attendance type. Use 'board' or 'deboard'.");
            }

            await _context.SaveChangesAsync();

            // Here we would typically trigger a Notification to the parent
            // await TriggerParentNotification(attendance.StudentId, attendance.Status);

            return Ok(attendance);
        }
    }

    public class MarkTransportAttendanceDto
    {
        public int StudentId { get; set; }
        public string Type { get; set; } = string.Empty; // "board" or "deboard"
        public string Method { get; set; } = "QR Scan";
    }
}
