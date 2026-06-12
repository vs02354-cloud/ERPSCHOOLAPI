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
    public class HomeworkController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HomeworkController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Teacher")]
        public async Task<ActionResult<Homework>> AssignHomework(Homework homework)
        {
            homework.AssignedDate = SchoolERP.Api.Utils.TimeUtils.GetIstTime();
            _context.Homeworks.Add(homework);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetHomework), new { id = homework.Id }, homework);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Homework>> GetHomework(int id)
        {
            var homework = await _context.Homeworks.FindAsync(id);
            if (homework == null) return NotFound();
            return homework;
        }

        [HttpGet("Class/{className}")]
        public async Task<ActionResult<IEnumerable<Homework>>> GetClassHomework(string className)
        {
            return await _context.Homeworks
                .Where(h => h.ClassName == className)
                .OrderByDescending(h => h.AssignedDate)
                .ToListAsync();
        }
    }
}
