using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Api.Data;
using SchoolERP.Api.Models;
using SchoolERP.Api.Models.DTOs;

namespace SchoolERP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CommunicationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CommunicationController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("Broadcast")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Principal,Teacher")]
        public async Task<IActionResult> SendBroadcast([FromBody] BroadcastRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Message cannot be empty.");
            }

            // Fetch valid parent contact numbers based on target audience
            var studentsQuery = _context.Students.Where(s => !string.IsNullOrEmpty(s.ParentContactNumber));

            if (request.TargetAudience == "SpecificClass" && !string.IsNullOrEmpty(request.TargetClass))
            {
                studentsQuery = studentsQuery.Where(s => s.CurrentClass == request.TargetClass);
            }

            var numbers = await studentsQuery
                .Select(s => s.ParentContactNumber)
                .Distinct()
                .ToListAsync();

            if (!numbers.Any())
            {
                return Ok(new { Count = 0, Status = "No parent numbers found." });
            }

            // SIMULATED: In a real environment, you would loop through 'numbers' 
            // and send the request.Message via Twilio (SMS or WhatsApp API).
            
            // Example Twilio snippet:
            // foreach(var num in numbers) {
            //    MessageResource.Create(body: request.Message, from: new Twilio.Types.PhoneNumber("+1234567890"), to: new Twilio.Types.PhoneNumber(num));
            // }

            // We'll log the simulated count and return success.
            return Ok(new { Count = numbers.Count, Status = "Success" });
        }

        [HttpPost("Notice")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Principal")]
        public async Task<ActionResult<Notice>> CreateNotice(Notice notice)
        {
            notice.PublishDate = SchoolERP.Api.Utils.TimeUtils.GetIstTime();
            _context.Notices.Add(notice);
            await _context.SaveChangesAsync();
            
            // Here you would integrate with SMS/Email services like Twilio or SendGrid
            
            return CreatedAtAction(nameof(GetNotice), new { id = notice.Id }, notice);
        }

        [HttpGet("Notice/{id}")]
        public async Task<ActionResult<Notice>> GetNotice(int id)
        {
            var notice = await _context.Notices.FindAsync(id);
            if (notice == null) return NotFound();
            return notice;
        }

        [HttpGet("Notice/Audience/{targetAudience}")]
        public async Task<ActionResult<IEnumerable<Notice>>> GetNoticesForAudience(string targetAudience)
        {
            return await _context.Notices
                .Where(n => n.TargetAudience == "All" || n.TargetAudience == targetAudience)
                .OrderByDescending(n => n.PublishDate)
                .ToListAsync();
        }
    }
}
