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
    public class TCController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TCController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Principal")]
        public async Task<ActionResult<IEnumerable<TransferCertificate>>> GetTransferCertificates()
        {
            return await _context.TransferCertificates
                .Include(tc => tc.Student)
                .OrderByDescending(tc => tc.IssueDate)
                .ToListAsync();
        }

        [HttpPost("Generate")]
        [Authorize(Roles = "Admin,Super Admin,School Admin")]
        public async Task<ActionResult<TransferCertificate>> GenerateTC([FromBody] TransferCertificate request)
        {
            var student = await _context.Students.FindAsync(request.StudentId);
            if (student == null) return NotFound("Student not found");
            if (!student.IsActive) return BadRequest("Student is already inactive or has a TC issued.");

            // Generate TC Number
            var count = await _context.TransferCertificates.CountAsync();
            request.TCNumber = $"TC{SchoolERP.Api.Utils.TimeUtils.GetIstTime().Year}{(count + 1):D4}";
            request.IssueDate = SchoolERP.Api.Utils.TimeUtils.GetIstTime();
            request.CreatedDate = SchoolERP.Api.Utils.TimeUtils.GetIstTime();

            // Save TC
            _context.TransferCertificates.Add(request);

            // Hide student from active list
            student.IsActive = false;
            
            await _context.SaveChangesAsync();

            return Ok(request);
        }
    }
}
