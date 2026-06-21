using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Api.Data;

namespace SchoolERP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("ActivityTrends")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Principal")]
        public async Task<ActionResult<IEnumerable<object>>> GetActivityTrends()
        {
            var today = Utils.TimeUtils.GetIstTime();
            var sixMonthsAgo = today.AddMonths(-5);
            var startDate = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1);

            // Fetch data
            var admissions = await _context.Students
                .Where(s => s.AdmissionDate >= startDate)
                .ToListAsync();

            var inquiries = await _context.AdmissionInquiries
                .Where(i => i.InquiryDate >= startDate)
                .ToListAsync();

            var leaves = await _context.LeaveRequests
                .Where(l => l.StartDate >= startDate)
                .ToListAsync();

            var trends = new List<object>();

            for (int i = 0; i < 6; i++)
            {
                var targetMonth = startDate.AddMonths(i);
                
                var admissionsCount = admissions.Count(a => a.AdmissionDate.Year == targetMonth.Year && a.AdmissionDate.Month == targetMonth.Month);
                var inquiriesCount = inquiries.Count(q => q.InquiryDate.Year == targetMonth.Year && q.InquiryDate.Month == targetMonth.Month);
                var leavesCount = leaves.Count(l => l.StartDate.Year == targetMonth.Year && l.StartDate.Month == targetMonth.Month);

                trends.Add(new
                {
                    Month = targetMonth.ToString("MMM"),
                    Admissions = admissionsCount,
                    Inquiries = inquiriesCount,
                    Leaves = leavesCount
                });
            }

            return Ok(trends);
        }
    }
}
