using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Api.Data;
using SchoolERP.Api.Models;

namespace SchoolERP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Super Admin,School Admin,Principal,Teacher")]
    public class PromotionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PromotionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public class PromoteRequestDto
        {
            public List<int> StudentIds { get; set; } = new();
            public string NextClass { get; set; } = string.Empty;
        }

        [HttpPost("Promote")]
        public async Task<IActionResult> PromoteStudents([FromBody] PromoteRequestDto request)
        {
            if (request.StudentIds == null || !request.StudentIds.Any())
                return BadRequest("No students selected for promotion.");
            
            if (string.IsNullOrWhiteSpace(request.NextClass))
                return BadRequest("Next class must be specified.");

            var students = await _context.Students
                .Where(s => request.StudentIds.Contains(s.Id))
                .ToListAsync();

            if (!students.Any())
                return NotFound("Selected students not found.");

            var userName = User.Identity?.Name ?? "Unknown";
            
            var promotions = new List<StudentPromotion>();

            foreach (var student in students)
            {
                var promotion = new StudentPromotion
                {
                    StudentId = student.Id,
                    FromClass = student.CurrentClass,
                    ToClass = request.NextClass,
                    PromotedBy = userName,
                    PromotionDate = SchoolERP.Api.Utils.TimeUtils.GetIstTime()
                };
                
                // Update current class
                student.CurrentClass = request.NextClass;
                
                promotions.Add(promotion);
            }

            _context.StudentPromotions.AddRange(promotions);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"{promotions.Count} students promoted successfully." });
        }

        [HttpGet("Report")]
        public async Task<ActionResult<IEnumerable<object>>> GetPromotionReport([FromQuery] string? classFilter = null)
        {
            var query = _context.StudentPromotions
                .Include(sp => sp.Student)
                .AsQueryable();

            if (!string.IsNullOrEmpty(classFilter))
            {
                query = query.Where(sp => sp.FromClass == classFilter || sp.ToClass == classFilter);
            }

            var report = await query
                .OrderByDescending(sp => sp.PromotionDate)
                .Select(sp => new 
                {
                    sp.Id,
                    StudentName = sp.Student.FirstName + " " + sp.Student.LastName,
                    sp.Student.AdmissionNumber,
                    sp.FromClass,
                    sp.ToClass,
                    sp.PromotionDate,
                    sp.PromotedBy
                })
                .ToListAsync();

            return Ok(report);
        }
    }
}
