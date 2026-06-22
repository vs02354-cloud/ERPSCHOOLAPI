using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Api.Data;

namespace SchoolERP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublicController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PublicController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("HomePageData")]
        public async Task<IActionResult> GetHomePageData()
        {
            var now = DateTime.UtcNow;

            var settings = await _context.HomePageSettings.FirstOrDefaultAsync();
            var quickLinks = await _context.QuickLinks.Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ToListAsync();
            var socialMedia = await _context.SocialMediaLinks.Where(x => x.IsActive).ToListAsync();
            var events = await _context.UpcomingEvents.Where(x => x.IsActive && x.EventDate >= now.Date).OrderBy(x => x.EventDate).Take(5).ToListAsync();
            var activities = await _context.RecentActivities.Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).Take(6).ToListAsync();
            var faculty = await _context.FacultyExcellences.Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ToListAsync();
            var students = await _context.StudentSpotlights.Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ToListAsync();
            var stats = await _context.HomeStatistics.Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ToListAsync();
            var portals = await _context.PortalCards.Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ToListAsync();
            var tickers = await _context.NewsTickers.Where(x => x.IsActive && x.StartDate <= now && x.ExpiryDate >= now).OrderByDescending(x => x.Priority).ToListAsync();
            var gallery = await _context.ImageGalleries.OrderBy(x => x.DisplayOrder).ToListAsync();

            return Ok(new
            {
                Settings = settings ?? new Models.HomePageSettings(),
                QuickLinks = quickLinks,
                SocialMediaLinks = socialMedia,
                UpcomingEvents = events,
                RecentActivities = activities,
                FacultyExcellences = faculty,
                StudentSpotlights = students,
                HomeStatistics = stats,
                PortalCards = portals,
                NewsTickers = tickers,
                ImageGallery = gallery
            });
        }
    }
}
