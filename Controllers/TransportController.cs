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
    public class TransportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TransportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- Vehicles ---

        [HttpPost("Vehicle")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Transport Manager")]
        public async Task<ActionResult<Vehicle>> AddVehicle(Vehicle vehicle)
        {
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.Id }, vehicle);
        }

        [HttpGet("Vehicle/{id}")]
        public async Task<ActionResult<Vehicle>> GetVehicle(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound();
            return vehicle;
        }

        // --- Routes ---

        [HttpPost("Route")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Transport Manager")]
        public async Task<ActionResult<TransportRoute>> AddRoute(TransportRoute route)
        {
            _context.TransportRoutes.Add(route);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetRoute), new { id = route.Id }, route);
        }

        [HttpGet("Route/{id}")]
        public async Task<ActionResult<TransportRoute>> GetRoute(int id)
        {
            var route = await _context.TransportRoutes.Include(r => r.Vehicle).FirstOrDefaultAsync(r => r.Id == id);
            if (route == null) return NotFound();
            return route;
        }

        [HttpGet("Route")]
        public async Task<ActionResult<IEnumerable<TransportRoute>>> GetAllRoutes()
        {
            return await _context.TransportRoutes.Include(r => r.Vehicle).ToListAsync();
        }
    }
}
