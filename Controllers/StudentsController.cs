using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Api.Data;
using SchoolERP.Api.Models;

namespace SchoolERP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requires authentication for all endpoints
    public class StudentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StudentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Students
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
        {
            return await _context.Students.Where(s => s.IsActive).ToListAsync();
        }

        // GET: api/Students/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> GetStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);

            if (student == null)
            {
                return NotFound();
            }

            return student;
        }

        // GET: api/Students/MyChildren
        [HttpGet("MyChildren")]
        [Authorize(Roles = "Parent")]
        public async Task<ActionResult<IEnumerable<Student>>> GetMyChildren()
        {
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var children = await _context.Students.Where(s => s.ParentUserId == userId && s.IsActive).ToListAsync();
            return Ok(children);
        }

        // POST: api/Students/Admission
        [HttpPost("Admission")]
        [Authorize(Roles = "Admin,Principal,Receptionist,Super Admin,School Admin")] // Updated to match actual DB roles
        public async Task<ActionResult<Student>> AdmitStudent(Student student)
        {
            // Auto generate Admission number based on year + count
            var count = await _context.Students.CountAsync();
            student.AdmissionNumber = $"ADM{DateTime.Now.Year}{(count + 1):D4}";
            student.AdmissionDate = DateTime.UtcNow;

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, student);
        }

        // PUT: api/Students/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudent(int id, Student student)
        {
            if (id != student.Id)
            {
                return BadRequest();
            }

            _context.Entry(student).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
}
