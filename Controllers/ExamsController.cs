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
    public class ExamsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ExamsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- Exams ---
        
        [HttpPost]
        [Authorize(Roles = "Admin,Super Admin,School Admin")]
        public async Task<ActionResult<Exam>> CreateExam(Exam exam)
        {
            _context.Exams.Add(exam);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetExam), new { id = exam.Id }, exam);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Exam>> GetExam(int id)
        {
            var exam = await _context.Exams.Include(e => e.Results).FirstOrDefaultAsync(e => e.Id == id);
            if (exam == null) return NotFound();
            return exam;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Exam>>> GetAllExams()
        {
            return await _context.Exams.ToListAsync();
        }

        // --- Exam Results ---

        [HttpPost("Result")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Teacher")]
        public async Task<ActionResult<ExamResult>> AddExamResult(ExamResult result)
        {
            _context.ExamResults.Add(result);
            await _context.SaveChangesAsync();
            return Ok(result);
        }

        [HttpGet("Result/Student/{studentId}")]
        public async Task<ActionResult<IEnumerable<ExamResult>>> GetStudentResults(int studentId)
        {
            return await _context.ExamResults
                .Include(r => r.Exam)
                .Where(r => r.StudentId == studentId)
                .ToListAsync();
        }
    }
}
