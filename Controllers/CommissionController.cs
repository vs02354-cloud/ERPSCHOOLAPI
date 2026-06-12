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
    public class CommissionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CommissionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- Commission Settings ---

        [HttpGet("Settings")]
        [Authorize(Roles = "Admin,Super Admin,School Admin")]
        public async Task<ActionResult<IEnumerable<CommissionSetting>>> GetSettings()
        {
            return await _context.CommissionSettings.ToListAsync();
        }

        [HttpPost("Settings")]
        [Authorize(Roles = "Admin,Super Admin,School Admin")]
        public async Task<ActionResult<CommissionSetting>> CreateSetting(CommissionSetting setting)
        {
            if (setting.IsActive)
            {
                var activeSettings = await _context.CommissionSettings.Where(c => c.IsActive).ToListAsync();
                foreach (var active in activeSettings)
                {
                    active.IsActive = false;
                    _context.Entry(active).State = EntityState.Modified;
                }
            }

            _context.CommissionSettings.Add(setting);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSettings), new { id = setting.Id }, setting);
        }

        [HttpPut("Settings/{id}")]
        [Authorize(Roles = "Admin,Super Admin,School Admin")]
        public async Task<IActionResult> UpdateSetting(int id, CommissionSetting setting)
        {
            if (id != setting.Id) return BadRequest();

            if (setting.IsActive)
            {
                var activeSettings = await _context.CommissionSettings.Where(c => c.IsActive && c.Id != id).ToListAsync();
                foreach (var active in activeSettings)
                {
                    active.IsActive = false;
                    _context.Entry(active).State = EntityState.Modified;
                }
            }

            _context.Entry(setting).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("Settings/{id}")]
        [Authorize(Roles = "Admin,Super Admin,School Admin")]
        public async Task<IActionResult> DeleteSetting(int id)
        {
            var setting = await _context.CommissionSettings.FindAsync(id);
            if (setting == null) return NotFound();

            _context.CommissionSettings.Remove(setting);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // --- Teacher Commissions ---

        [HttpGet("Reports")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Principal,Accountant")]
        public async Task<ActionResult<IEnumerable<object>>> GetCommissionReports(
            [FromQuery] int? teacherId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var query = _context.TeacherCommissions
                .Include(tc => tc.Employee)
                .Include(tc => tc.Student)
                .Include(tc => tc.FeePayment)
                .AsQueryable();

            if (teacherId.HasValue)
            {
                query = query.Where(tc => tc.EmployeeId == teacherId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(tc => tc.DateEarned >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                // Include entire day of end date
                var adjustedEndDate = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(tc => tc.DateEarned <= adjustedEndDate);
            }

            var commissions = await query.OrderByDescending(tc => tc.DateEarned).ToListAsync();

            var result = commissions.Select(tc => new
            {
                tc.Id,
                tc.EmployeeId,
                TeacherName = tc.Employee != null ? tc.Employee.FirstName + " " + tc.Employee.LastName : "Unknown",
                tc.StudentId,
                StudentName = tc.Student != null ? tc.Student.FirstName + " " + tc.Student.LastName : "Unknown",
                tc.FeePaymentId,
                ReceiptNumber = tc.FeePayment != null ? tc.FeePayment.ReceiptNumber : "Unknown",
                tc.CommissionAmount,
                tc.DateEarned,
                tc.IsPaid
            });

            return Ok(result);
        }

        [HttpPut("Reports/{id}/Pay")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Accountant")]
        public async Task<IActionResult> MarkCommissionAsPaid(int id)
        {
            var commission = await _context.TeacherCommissions.FindAsync(id);
            if (commission == null) return NotFound();

            commission.IsPaid = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
