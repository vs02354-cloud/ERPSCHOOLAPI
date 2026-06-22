using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Api.Data;
using SchoolERP.Api.Models;
using System.Text.Json;

namespace SchoolERP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Super Admin,School Admin")]
    public class CmsAdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CmsAdminController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private async Task LogAction(string action, string module, object prevValue, object newValue)
        {
            var log = new CmsAuditLog
            {
                UserName = User.Identity?.Name ?? "Admin",
                Action = action,
                Module = module,
                Timestamp = DateTime.UtcNow,
                PreviousValue = prevValue != null ? JsonSerializer.Serialize(prevValue) : null,
                NewValue = newValue != null ? JsonSerializer.Serialize(newValue) : null
            };
            _context.CmsAuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        [HttpPost("UploadImage")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("No file uploaded");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "cms");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { Url = $"/uploads/cms/{fileName}" });
        }

        // --- HomePageSettings ---
        [HttpGet("Settings")]
        public async Task<ActionResult<HomePageSettings>> GetSettings()
        {
            var settings = await _context.HomePageSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new HomePageSettings();
                _context.HomePageSettings.Add(settings);
                await _context.SaveChangesAsync();
            }
            return settings;
        }

        [HttpPut("Settings")]
        public async Task<IActionResult> UpdateSettings(HomePageSettings updatedSettings)
        {
            var existing = await _context.HomePageSettings.FirstOrDefaultAsync();
            if (existing == null) return NotFound();

            var oldValues = JsonSerializer.Deserialize<HomePageSettings>(JsonSerializer.Serialize(existing));

            existing.SchoolName = updatedSettings.SchoolName;
            existing.LogoUrl = updatedSettings.LogoUrl;
            existing.Address = updatedSettings.Address;
            existing.Email = updatedSettings.Email;
            existing.Phone = updatedSettings.Phone;
            existing.WebsiteUrl = updatedSettings.WebsiteUrl;
            existing.HeroTagline = updatedSettings.HeroTagline;
            existing.HeroHeading = updatedSettings.HeroHeading;
            existing.HeroDescription = updatedSettings.HeroDescription;
            existing.HeroPrimaryButtonText = updatedSettings.HeroPrimaryButtonText;
            existing.HeroPrimaryButtonUrl = updatedSettings.HeroPrimaryButtonUrl;
            existing.HeroSecondaryButtonText = updatedSettings.HeroSecondaryButtonText;
            existing.HeroSecondaryButtonUrl = updatedSettings.HeroSecondaryButtonUrl;
            existing.MapEmbedUrl = updatedSettings.MapEmbedUrl;

            await _context.SaveChangesAsync();
            await LogAction("Update", "Settings", oldValues, existing);
            return NoContent();
        }

        // Generic pattern for other CMS collections to keep code compact...

        // --- QuickLinks ---
        [HttpGet("QuickLinks")]
        public async Task<ActionResult<IEnumerable<QuickLink>>> GetQuickLinks() => await _context.QuickLinks.OrderBy(x => x.DisplayOrder).ToListAsync();
        
        [HttpPost("QuickLinks")]
        public async Task<ActionResult<QuickLink>> AddQuickLink(QuickLink item)
        {
            _context.QuickLinks.Add(item);
            await _context.SaveChangesAsync();
            await LogAction("Add", "QuickLinks", null, item);
            return CreatedAtAction(nameof(GetQuickLinks), new { id = item.Id }, item);
        }

        [HttpPut("QuickLinks/{id}")]
        public async Task<IActionResult> UpdateQuickLink(int id, QuickLink item)
        {
            if (id != item.Id) return BadRequest();
            var existing = await _context.QuickLinks.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            await LogAction("Update", "QuickLinks", existing, item);
            return NoContent();
        }

        [HttpDelete("QuickLinks/{id}")]
        public async Task<IActionResult> DeleteQuickLink(int id)
        {
            var item = await _context.QuickLinks.FindAsync(id);
            if (item == null) return NotFound();
            _context.QuickLinks.Remove(item);
            await _context.SaveChangesAsync();
            await LogAction("Delete", "QuickLinks", item, null);
            return NoContent();
        }

        // --- SocialMediaLinks ---
        [HttpGet("SocialMediaLinks")]
        public async Task<ActionResult<IEnumerable<SocialMediaLink>>> GetSocialMediaLinks() => await _context.SocialMediaLinks.ToListAsync();

        [HttpPost("SocialMediaLinks")]
        public async Task<ActionResult<SocialMediaLink>> AddSocialMediaLink(SocialMediaLink item)
        {
            _context.SocialMediaLinks.Add(item);
            await _context.SaveChangesAsync();
            await LogAction("Add", "SocialMediaLinks", null, item);
            return CreatedAtAction(nameof(GetSocialMediaLinks), new { id = item.Id }, item);
        }

        [HttpPut("SocialMediaLinks/{id}")]
        public async Task<IActionResult> UpdateSocialMediaLink(int id, SocialMediaLink item)
        {
            if (id != item.Id) return BadRequest();
            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- UpcomingEvents ---
        [HttpGet("UpcomingEvents")]
        public async Task<ActionResult<IEnumerable<UpcomingEvent>>> GetUpcomingEvents() => await _context.UpcomingEvents.OrderBy(x => x.EventDate).ToListAsync();

        [HttpPost("UpcomingEvents")]
        public async Task<ActionResult<UpcomingEvent>> AddUpcomingEvent(UpcomingEvent item)
        {
            _context.UpcomingEvents.Add(item);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUpcomingEvents), new { id = item.Id }, item);
        }

        [HttpPut("UpcomingEvents/{id}")]
        public async Task<IActionResult> UpdateUpcomingEvent(int id, UpcomingEvent item)
        {
            if (id != item.Id) return BadRequest();
            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("UpcomingEvents/{id}")]
        public async Task<IActionResult> DeleteUpcomingEvent(int id)
        {
            var item = await _context.UpcomingEvents.FindAsync(id);
            if (item == null) return NotFound();
            _context.UpcomingEvents.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- RecentActivities ---
        [HttpGet("RecentActivities")]
        public async Task<ActionResult<IEnumerable<RecentActivity>>> GetRecentActivities() => await _context.RecentActivities.OrderBy(x => x.DisplayOrder).ToListAsync();

        [HttpPost("RecentActivities")]
        public async Task<ActionResult<RecentActivity>> AddRecentActivity(RecentActivity item)
        {
            _context.RecentActivities.Add(item);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetRecentActivities), new { id = item.Id }, item);
        }

        [HttpPut("RecentActivities/{id}")]
        public async Task<IActionResult> UpdateRecentActivity(int id, RecentActivity item)
        {
            if (id != item.Id) return BadRequest();
            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }
        
        [HttpDelete("RecentActivities/{id}")]
        public async Task<IActionResult> DeleteRecentActivity(int id)
        {
            var item = await _context.RecentActivities.FindAsync(id);
            if (item == null) return NotFound();
            _context.RecentActivities.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- FacultyExcellences ---
        [HttpGet("FacultyExcellences")]
        public async Task<ActionResult<IEnumerable<FacultyExcellence>>> GetFacultyExcellences() => await _context.FacultyExcellences.OrderBy(x => x.DisplayOrder).ToListAsync();

        [HttpPost("FacultyExcellences")]
        public async Task<ActionResult<FacultyExcellence>> AddFacultyExcellence(FacultyExcellence item)
        {
            _context.FacultyExcellences.Add(item);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetFacultyExcellences), new { id = item.Id }, item);
        }

        [HttpPut("FacultyExcellences/{id}")]
        public async Task<IActionResult> UpdateFacultyExcellence(int id, FacultyExcellence item)
        {
            if (id != item.Id) return BadRequest();
            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }
        
        [HttpDelete("FacultyExcellences/{id}")]
        public async Task<IActionResult> DeleteFacultyExcellence(int id)
        {
            var item = await _context.FacultyExcellences.FindAsync(id);
            if (item == null) return NotFound();
            _context.FacultyExcellences.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- StudentSpotlights ---
        [HttpGet("StudentSpotlights")]
        public async Task<ActionResult<IEnumerable<StudentSpotlight>>> GetStudentSpotlights() => await _context.StudentSpotlights.OrderBy(x => x.DisplayOrder).ToListAsync();

        [HttpPost("StudentSpotlights")]
        public async Task<ActionResult<StudentSpotlight>> AddStudentSpotlight(StudentSpotlight item)
        {
            _context.StudentSpotlights.Add(item);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetStudentSpotlights), new { id = item.Id }, item);
        }

        [HttpPut("StudentSpotlights/{id}")]
        public async Task<IActionResult> UpdateStudentSpotlight(int id, StudentSpotlight item)
        {
            if (id != item.Id) return BadRequest();
            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }
        
        [HttpDelete("StudentSpotlights/{id}")]
        public async Task<IActionResult> DeleteStudentSpotlight(int id)
        {
            var item = await _context.StudentSpotlights.FindAsync(id);
            if (item == null) return NotFound();
            _context.StudentSpotlights.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- HomeStatistics ---
        [HttpGet("HomeStatistics")]
        public async Task<ActionResult<IEnumerable<HomeStatistic>>> GetHomeStatistics() => await _context.HomeStatistics.OrderBy(x => x.DisplayOrder).ToListAsync();

        [HttpPost("HomeStatistics")]
        public async Task<ActionResult<HomeStatistic>> AddHomeStatistic(HomeStatistic item)
        {
            _context.HomeStatistics.Add(item);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetHomeStatistics), new { id = item.Id }, item);
        }

        [HttpPut("HomeStatistics/{id}")]
        public async Task<IActionResult> UpdateHomeStatistic(int id, HomeStatistic item)
        {
            if (id != item.Id) return BadRequest();
            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("HomeStatistics/{id}")]
        public async Task<IActionResult> DeleteHomeStatistic(int id)
        {
            var item = await _context.HomeStatistics.FindAsync(id);
            if (item == null) return NotFound();
            _context.HomeStatistics.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- NewsTickers ---
        [HttpGet("NewsTickers")]
        public async Task<ActionResult<IEnumerable<NewsTicker>>> GetNewsTickers() => await _context.NewsTickers.OrderByDescending(x => x.Priority).ToListAsync();

        [HttpPost("NewsTickers")]
        public async Task<ActionResult<NewsTicker>> AddNewsTicker(NewsTicker item)
        {
            _context.NewsTickers.Add(item);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetNewsTickers), new { id = item.Id }, item);
        }

        [HttpPut("NewsTickers/{id}")]
        public async Task<IActionResult> UpdateNewsTicker(int id, NewsTicker item)
        {
            if (id != item.Id) return BadRequest();
            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("NewsTickers/{id}")]
        public async Task<IActionResult> DeleteNewsTicker(int id)
        {
            var item = await _context.NewsTickers.FindAsync(id);
            if (item == null) return NotFound();
            _context.NewsTickers.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- PortalCards ---
        [HttpGet("PortalCards")]
        public async Task<ActionResult<IEnumerable<PortalCard>>> GetPortalCards() => await _context.PortalCards.OrderBy(x => x.DisplayOrder).ToListAsync();

        [HttpPost("PortalCards")]
        public async Task<ActionResult<PortalCard>> AddPortalCard(PortalCard item)
        {
            _context.PortalCards.Add(item);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPortalCards), new { id = item.Id }, item);
        }

        [HttpPut("PortalCards/{id}")]
        public async Task<IActionResult> UpdatePortalCard(int id, PortalCard item)
        {
            if (id != item.Id) return BadRequest();
            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("PortalCards/{id}")]
        public async Task<IActionResult> DeletePortalCard(int id)
        {
            var item = await _context.PortalCards.FindAsync(id);
            if (item == null) return NotFound();
            _context.PortalCards.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- Audit Logs ---
        [HttpGet("Logs")]
        public async Task<ActionResult<IEnumerable<CmsAuditLog>>> GetAuditLogs()
        {
            return await _context.CmsAuditLogs.OrderByDescending(x => x.Timestamp).Take(50).ToListAsync();
        }
    }
}
