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
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public CommunicationController(ApplicationDbContext context, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
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

                // Process in background to avoid long UI delays
                var messageContent = request.Message;
                _ = Task.Run(async () =>
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

                    var emailSettings = config.GetSection("EmailSettings");
                    string smtpServer = emailSettings["SmtpServer"] ?? "smtp.gmail.com";
                    int smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
                    string senderEmail = emailSettings["SenderEmail"] ?? "your-email@gmail.com";
                    string senderName = emailSettings["SenderName"] ?? "School Admin";
                    string smtpUsername = emailSettings["Username"] ?? "";
                    string smtpPassword = emailSettings["Password"] ?? "";

                    using var smtpClient = new System.Net.Mail.SmtpClient(smtpServer, smtpPort)
                    {
                        Credentials = new System.Net.NetworkCredential(smtpUsername, smtpPassword),
                        EnableSsl = true
                    };

                    foreach (var student in studentsWithEmail)
                    {
                        var log = new EmailDeliveryLog
                        {
                            StudentId = student.Id,
                            EmailAddress = student.ParentEmail,
                            MessageContent = messageContent,
                            Status = "Pending"
                        };

                        try
                        {
                            var mailMessage = new System.Net.Mail.MailMessage
                            {
                                From = new System.Net.Mail.MailAddress(senderEmail, senderName),
                                Subject = "School Broadcast Message",
                                Body = $"<p>Dear Parent,</p><p>{messageContent}</p>",
                                IsBodyHtml = true
                            };
                            mailMessage.To.Add(student.ParentEmail);

                            await smtpClient.SendMailAsync(mailMessage);

                            log.Status = "Delivered";
                        }
                        catch (Exception ex)
                        {
                            log.Status = "Failed";
                            log.ErrorMessage = ex.Message;
                        }

                        dbContext.EmailDeliveryLogs.Add(log);
                    }

                    await dbContext.SaveChangesAsync();
                });

                return Ok(new { Count = studentsWithEmail.Count, Status = "Success" });
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
