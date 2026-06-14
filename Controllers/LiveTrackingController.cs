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
    public class LiveTrackingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LiveTrackingController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("vehicle/{vehicleId}")]
        public async Task<ActionResult<DriverLocation>> GetVehicleLocation(int vehicleId)
        {
            var location = await _context.DriverLocations
                .Where(l => l.VehicleId == vehicleId && l.IsActive)
                .OrderByDescending(l => l.Timestamp)
                .FirstOrDefaultAsync();

            if (location == null) return NotFound("No active tracking session found for this vehicle");

            return location;
        }

        [HttpPost("update")]
        [Authorize(Roles = "Driver,Attendant,Admin,Super Admin,School Admin,Transport Manager")]
        public async Task<ActionResult> UpdateLocation([FromBody] UpdateLocationDto dto)
        {
            var userName = User.Identity?.Name;
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Username == userName);
            
            if (employee == null && !User.IsInRole("Admin")) 
                return Unauthorized();

            var location = new DriverLocation
            {
                VehicleId = dto.VehicleId,
                DriverEmployeeId = employee?.Id ?? 0, // Fallback if admin forces update
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                SpeedKmH = dto.SpeedKmH,
                Timestamp = SchoolERP.Api.Utils.TimeUtils.GetIstTime(),
                IsActive = true
            };

            // Deactivate older locations for this vehicle to keep data clean, or just append
            // We'll just append for historical tracking, and the Get endpoint fetches the latest
            
            _context.DriverLocations.Add(location);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }

    public class UpdateLocationDto
    {
        public int VehicleId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double SpeedKmH { get; set; }
    }
}
