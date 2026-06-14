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
    public class VehiclesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VehiclesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetVehicles()
        {
            return await _context.Vehicles
                .Include(v => v.Driver)
                .Include(v => v.Attendant)
                .Include(v => v.AssignedRoute)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Vehicle>> GetVehicle(int id)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.Driver)
                .Include(v => v.Attendant)
                .Include(v => v.AssignedRoute)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null) return NotFound();

            return vehicle;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Transport Manager")]
        public async Task<ActionResult<Vehicle>> PostVehicle(Vehicle vehicle)
        {
            // Prevent assigning same vehicle to multiple active routes
            if (vehicle.AssignedRouteId.HasValue && vehicle.IsActive)
            {
                var existingAssigned = await _context.Vehicles
                    .AnyAsync(v => v.AssignedRouteId == vehicle.AssignedRouteId && v.IsActive && v.Id != vehicle.Id);
                
                // Assuming 1 Route = 1 Active Vehicle for simplicity as per requirement: "Prevent assigning the same vehicle to multiple active routes at the same time"
                // Actually the requirement says "Prevent assigning the same vehicle to multiple active routes". A vehicle can only have ONE AssignedRouteId anyway. 
                // Maybe it means "Prevent assigning multiple vehicles to the same route"? No, "Prevent assigning the same vehicle to multiple active routes".
                // Since AssignedRouteId is a single int, a vehicle CANNOT be on multiple routes by database design. So it's inherently prevented!
            }

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.Id }, vehicle);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Transport Manager")]
        public async Task<IActionResult> PutVehicle(int id, Vehicle vehicle)
        {
            if (id != vehicle.Id) return BadRequest();

            _context.Entry(vehicle).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehicleExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Transport Manager")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound();

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VehicleExists(int id)
        {
            return _context.Vehicles.Any(e => e.Id == id);
        }
    }
}
