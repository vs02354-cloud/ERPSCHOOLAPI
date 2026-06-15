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
    public class TransportGatePassController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TransportGatePassController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Transport Manager")]
        public async Task<ActionResult<IEnumerable<TransportGatePass>>> GetGatePasses()
        {
            return await _context.TransportGatePasses
                .Include(g => g.Student)
                .Include(g => g.Route)
                .Include(g => g.Vehicle)
                .ToListAsync();
        }

        [HttpGet("my-gatepass")]
        public async Task<ActionResult<TransportGatePass>> GetMyGatePass()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
            
            if (student == null) return NotFound("Student not found");

            var gatePass = await _context.TransportGatePasses
                .Include(g => g.Route)
                .Include(g => g.Vehicle)
                .FirstOrDefaultAsync(g => g.StudentId == student.Id && g.IsActive && g.ValidUntil >= SchoolERP.Api.Utils.TimeUtils.GetIstTime());

            if (gatePass == null) return NotFound("Active gate pass not found");

            return gatePass;
        }

        [HttpPost("generate/{studentIdentifier}")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Transport Manager")]
        public async Task<ActionResult<TransportGatePass>> GenerateGatePass(string studentIdentifier)
        {
            bool isNumeric = int.TryParse(studentIdentifier, out int parsedId);
            var student = await _context.Students.Include(s => s.TransportRouteStop)
                .FirstOrDefaultAsync(s => (isNumeric && s.Id == parsedId) || s.AdmissionNumber == studentIdentifier);
            if (student == null || !student.TransportRequired || student.TransportRouteStopId == null)
            {
                return BadRequest("Student is not enrolled in transport");
            }

            var routeId = student.TransportRouteStop!.RouteId;
            var route = await _context.TransportRoutes.Include(r => r.Vehicles).FirstOrDefaultAsync(r => r.Id == routeId);
            
            if (route == null || !route.Vehicles.Any(v => v.IsActive))
            {
                return BadRequest("No active vehicle assigned to this route");
            }

            var vehicleId = route.Vehicles.First(v => v.IsActive).Id;

            // Invalidate old passes
            var oldPasses = await _context.TransportGatePasses.Where(g => g.StudentId == student.Id && g.IsActive).ToListAsync();
            foreach (var op in oldPasses) op.IsActive = false;

            // Generate new QR Data (Guid is simple and secure enough for mapping)
            var qrCodeData = Guid.NewGuid().ToString();

            var gatePass = new TransportGatePass
            {
                StudentId = student.Id,
                RouteId = routeId,
                VehicleId = vehicleId,
                QRCodeData = qrCodeData,
                ValidUntil = SchoolERP.Api.Utils.TimeUtils.GetIstTime().AddYears(1), // Valid for academic year
                IsActive = true
            };

            _context.TransportGatePasses.Add(gatePass);
            await _context.SaveChangesAsync();

            return Ok(gatePass);
        }

        [HttpGet("verify/{qrCodeData}")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Transport Manager,Driver,Attendant")]
        public async Task<ActionResult> VerifyGatePass(string qrCodeData)
        {
            var pass = await _context.TransportGatePasses
                .Include(p => p.Student)
                .Include(p => p.Route)
                .FirstOrDefaultAsync(p => p.QRCodeData == qrCodeData);

            if (pass == null) return NotFound("Invalid QR Code");
            if (!pass.IsActive) return BadRequest("Gate Pass is deactivated");
            if (pass.ValidUntil < SchoolERP.Api.Utils.TimeUtils.GetIstTime()) return BadRequest("Gate Pass has expired");

            return Ok(new
            {
                IsValid = true,
                StudentName = pass.Student!.FirstName + " " + pass.Student.LastName,
                AdmissionNumber = pass.Student.AdmissionNumber,
                RouteName = pass.Route!.RouteName
            });
        }

        [HttpPut("revoke/{id}")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Transport Manager")]
        public async Task<IActionResult> RevokeGatePass(int id)
        {
            var pass = await _context.TransportGatePasses.FindAsync(id);
            if (pass == null) return NotFound("Gate Pass not found");

            pass.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
