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
    public class TransportRoutesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TransportRoutesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/TransportRoutes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransportRoute>>> GetTransportRoutes()
        {
            return await _context.TransportRoutes
                .Include(r => r.RouteStops)
                .Include(r => r.Vehicles)
                .ToListAsync();
        }

        // GET: api/TransportRoutes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TransportRoute>> GetTransportRoute(int id)
        {
            var route = await _context.TransportRoutes
                .Include(r => r.RouteStops)
                .Include(r => r.Vehicles)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (route == null) return NotFound();

            return route;
        }

        // POST: api/TransportRoutes
        [HttpPost]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Transport Manager")]
        public async Task<ActionResult<TransportRoute>> PostTransportRoute(TransportRoute route)
        {
            _context.TransportRoutes.Add(route);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTransportRoute), new { id = route.Id }, route);
        }

        // PUT: api/TransportRoutes/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Transport Manager")]
        public async Task<IActionResult> PutTransportRoute(int id, TransportRoute route)
        {
            if (id != route.Id) return BadRequest();

            _context.Entry(route).State = EntityState.Modified;

            // Handle RouteStops updates if needed, simple approach: just update the route main fields for now
            // More complex logic required to sync RouteStops

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TransportRouteExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // DELETE: api/TransportRoutes/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Transport Manager")]
        public async Task<IActionResult> DeleteTransportRoute(int id)
        {
            var route = await _context.TransportRoutes.FindAsync(id);
            if (route == null) return NotFound();

            _context.TransportRoutes.Remove(route);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // --- Route Stops Management ---

        [HttpPost("{routeId}/stops")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Transport Manager")]
        public async Task<ActionResult<TransportRouteStop>> AddRouteStop(int routeId, TransportRouteStop stop)
        {
            if (routeId != stop.RouteId) return BadRequest();
            
            var route = await _context.TransportRoutes.FindAsync(routeId);
            if (route == null) return NotFound("Route not found");

            _context.TransportRouteStops.Add(stop);
            await _context.SaveChangesAsync();

            return Ok(stop);
        }

        [HttpDelete("{routeId}/stops/{stopId}")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Transport Manager")]
        public async Task<IActionResult> DeleteRouteStop(int routeId, int stopId)
        {
            var stop = await _context.TransportRouteStops.FirstOrDefaultAsync(s => s.Id == stopId && s.RouteId == routeId);
            if (stop == null) return NotFound();

            _context.TransportRouteStops.Remove(stop);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TransportRouteExists(int id)
        {
            return _context.TransportRoutes.Any(e => e.Id == id);
        }
    }
}
