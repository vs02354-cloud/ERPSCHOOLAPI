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
    public class SystemActivityController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SystemActivityController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("recent")]
        public async Task<ActionResult<IEnumerable<SystemActivity>>> GetRecentActivities([FromQuery] int limit = 20)
        {
            return await _context.SystemActivities
                .OrderByDescending(a => a.Timestamp)
                .Take(limit)
                .ToListAsync();
        }
    }
}
