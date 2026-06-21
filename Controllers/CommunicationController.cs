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

            var studentsQuery = _context.Students.AsQueryable();

            if (request.TargetAudience == "SpecificClass" && !string.IsNullOrEmpty(request.TargetClass))
            {
                studentsQuery = studentsQuery.Where(s => s.CurrentClass == request.TargetClass);
            }

            if (request.Type == "Email")
            {
                var studentsWithEmail = await studentsQuery
                    .Where(s => !string.IsNullOrEmpty(s.ParentEmail))
                    .ToListAsync();

                if (!studentsWithEmail.Any())
                {
                    return Ok(new { Count = 0, Status = "No parent emails found." });
                }

                int successCount = 0;
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "re_BBytkH56_9sG1XQEbzA5XEWxq5191HGRv");
                httpClient.DefaultRequestHeaders.Add("accept", "application/json");

                foreach (var student in studentsWithEmail)
                {
                    var log = new EmailDeliveryLog
                    {
                        StudentId = student.Id,
                        EmailAddress = student.ParentEmail,
                        MessageContent = request.Message,
                        Status = "Pending"
                    };

                    try
                    {
                        var payload = new
                        {
                            from = "School Admin <onboarding@resend.dev>",
                            to = new[] { student.ParentEmail },
                            subject = "School Broadcast Message",
                            html = $"<p>Dear Parent,</p><p>{request.Message}</p>"
                        };

                        var response = await httpClient.PostAsJsonAsync("https://api.resend.com/emails", payload);

                        if (response.IsSuccessStatusCode)
                        {
                            log.Status = "Delivered";
                            successCount++;
                        }
                        else
                        {
                            log.Status = "Failed";
                            log.ErrorMessage = await response.Content.ReadAsStringAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Status = "Failed";
                        log.ErrorMessage = ex.Message;
                    }

                    _context.EmailDeliveryLogs.Add(log);
                }

                await _context.SaveChangesAsync();
                return Ok(new { Count = successCount, Status = "Success" });
            }

            // Fallback for SMS/WhatsApp
            var numbers = await studentsQuery
                .Where(s => !string.IsNullOrEmpty(s.ParentContactNumber))
                .Select(s => s.ParentContactNumber)
                .Distinct()
                .ToListAsync();

            if (!numbers.Any())
            {
                return Ok(new { Count = 0, Status = "No parent numbers found." });
            }

            // SIMULATED: SMS/WhatsApp delivery logic here...

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
