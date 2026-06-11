using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Api.Data;
using SchoolERP.Api.Models;
using System.Security.Claims;

namespace SchoolERP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotificationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("my-notifications")]
        public async Task<ActionResult<IEnumerable<Notification>>> GetMyNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.DateSent)
                .ToListAsync();

            return Ok(notifications);
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null) return NotFound();

            if (notification.UserId != userId) return Forbid();

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Notification marked as read." });
        }

        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (unreadNotifications.Any())
            {
                foreach (var notif in unreadNotifications)
                {
                    notif.IsRead = true;
                }
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "All notifications marked as read." });
        }

        [HttpPost("send")]
        [Authorize(Roles = "Admin,Super Admin,Principal,School Admin")]
        public async Task<IActionResult> SendNotification([FromBody] SendNotificationDto dto)
        {
            var senderId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(senderId)) return Unauthorized();

            if (dto.UserIds == null || !dto.UserIds.Any())
            {
                return BadRequest("No recipients selected.");
            }

            var notifications = dto.UserIds.Select(userId => new Notification
            {
                Title = dto.Title,
                Message = dto.Message,
                UserId = userId,
                SenderId = senderId,
                DateSent = DateTime.UtcNow,
                IsRead = false
            }).ToList();

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Notification sent to {notifications.Count} users successfully." });
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin,Super Admin,Principal,School Admin")]
        public async Task<IActionResult> GetUsersByRole([FromQuery] string role)
        {
            var users = await _context.Users
                .Where(u => u.UserType == role && u.IsActive)
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.UserName,
                    u.Email
                })
                .ToListAsync();

            return Ok(users);
        }
    }

    public class SendNotificationDto
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public List<string> UserIds { get; set; } = new List<string>();
    }
}
