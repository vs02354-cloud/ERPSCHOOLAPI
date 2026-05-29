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
    public class TimetableController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TimetableController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Super Admin,School Admin")]
        public async Task<ActionResult<Timetable>> CreateTimetableEntry(Timetable entry)
        {
            _context.Timetables.Add(entry);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEntry), new { id = entry.Id }, entry);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Timetable>> GetEntry(int id)
        {
            var entry = await _context.Timetables.FindAsync(id);
            if (entry == null) return NotFound();
            return entry;
        }

        [HttpGet("Class/{className}")]
        public async Task<ActionResult<IEnumerable<Timetable>>> GetClassTimetable(string className)
        {
            return await _context.Timetables
                .Where(t => t.ClassName == className)
                .OrderBy(t => t.DayOfWeek).ThenBy(t => t.Period)
                .ToListAsync();
        }

        [HttpGet("Teacher/{teacherId}")]
        public async Task<ActionResult<IEnumerable<Timetable>>> GetTeacherTimetable(string teacherId)
        {
            return await _context.Timetables
                .Where(t => t.TeacherId == teacherId)
                .OrderBy(t => t.DayOfWeek).ThenBy(t => t.Period)
                .ToListAsync();
        }
    }
}
