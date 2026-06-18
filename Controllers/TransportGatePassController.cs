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
                .OrderByDescending(g => g.CreatedDate)
                .ToListAsync();
        }

        [HttpGet("my-gatepass")]
        public async Task<ActionResult<IEnumerable<TransportGatePass>>> GetMyGatePasses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity?.Name;

            var students = await _context.Students
                .Where(s => s.ApplicationUserId == userId || s.ParentUserId == userId || s.ParentContactNumber == userName)
                .Select(s => s.Id)
                .ToListAsync();
            
            if (!students.Any()) return NotFound("Student not found");

            var gatePasses = await _context.TransportGatePasses
                .Include(g => g.Route)
                .Include(g => g.Vehicle)
                .Include(g => g.Student)
                .Where(g => students.Contains(g.StudentId))
                .OrderByDescending(g => g.CreatedDate)
                .ToListAsync();

            return gatePasses;
        }

        public class RemarksDto { public string Remarks { get; set; } = string.Empty; }

        [HttpPost("request")]
        public async Task<ActionResult<TransportGatePass>> RequestGatePass([FromQuery] int? studentId = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.Identity?.Name;
            
            var studentQuery = _context.Students.Include(s => s.TransportRouteStop)
                .Where(s => s.ApplicationUserId == userId || s.ParentUserId == userId || s.ParentContactNumber == userName);
                
            var student = studentId.HasValue 
                ? await studentQuery.FirstOrDefaultAsync(s => s.Id == studentId.Value)
                : await studentQuery.FirstOrDefaultAsync();
            
            if (student == null) return NotFound("Student not found");
            
            if (!student.TransportRequired || student.TransportRouteStopId == null)
            {
                return BadRequest("Transport facility is not enabled for this student.");
            }

            var routeId = student.TransportRouteStop!.RouteId;
            var route = await _context.TransportRoutes.Include(r => r.Vehicles).FirstOrDefaultAsync(r => r.Id == routeId);
            
            if (route == null || !route.Vehicles.Any(v => v.IsActive))
            {
                return BadRequest("No active vehicle assigned to this route");
            }

            var vehicleId = route.Vehicles.First(v => v.IsActive).Id;

            var qrCodeData = Guid.NewGuid().ToString();

            var gatePass = new TransportGatePass
            {
                StudentId = student.Id,
                RouteId = routeId,
                VehicleId = vehicleId,
                QRCodeData = qrCodeData,
                ValidUntil = SchoolERP.Api.Utils.TimeUtils.GetIstTime().AddYears(1), // Valid for academic year
                IsActive = true,
                Status = GatePassStatus.Pending,
                CreatedBy = "Parent",
                CreatedDate = SchoolERP.Api.Utils.TimeUtils.GetIstTime()
            };

            _context.TransportGatePasses.Add(gatePass);
            await _context.SaveChangesAsync();

            return Ok(gatePass);
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

            // Generate new QR Data
            var qrCodeData = Guid.NewGuid().ToString();
            var currentUser = User.FindFirstValue(ClaimTypes.Name);

            var gatePass = new TransportGatePass
            {
                StudentId = student.Id,
                RouteId = routeId,
                VehicleId = vehicleId,
                QRCodeData = qrCodeData,
                ValidUntil = SchoolERP.Api.Utils.TimeUtils.GetIstTime().AddYears(1),
                IsActive = true,
                Status = GatePassStatus.Approved,
                CreatedBy = currentUser ?? "Admin",
                CreatedDate = SchoolERP.Api.Utils.TimeUtils.GetIstTime(),
                ApprovedBy = currentUser ?? "Admin",
                ApprovalDate = SchoolERP.Api.Utils.TimeUtils.GetIstTime()
            };

            _context.TransportGatePasses.Add(gatePass);
            await _context.SaveChangesAsync();

            return Ok(gatePass);
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Transport Manager")]
        public async Task<IActionResult> ApproveGatePass(int id, [FromBody] RemarksDto dto)
        {
            var pass = await _context.TransportGatePasses.FindAsync(id);
            if (pass == null) return NotFound("Gate Pass not found");

            // Invalidate old passes
            var oldPasses = await _context.TransportGatePasses
                .Where(g => g.StudentId == pass.StudentId && g.Id != id && g.IsActive).ToListAsync();
            foreach (var op in oldPasses) op.IsActive = false;

            pass.Status = GatePassStatus.Approved;
            pass.Remarks = dto?.Remarks;
            pass.ApprovedBy = User.FindFirstValue(ClaimTypes.Name) ?? "Admin";
            pass.ApprovalDate = SchoolERP.Api.Utils.TimeUtils.GetIstTime();

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Transport Manager")]
        public async Task<IActionResult> RejectGatePass(int id, [FromBody] RemarksDto dto)
        {
            var pass = await _context.TransportGatePasses.FindAsync(id);
            if (pass == null) return NotFound("Gate Pass not found");

            pass.Status = GatePassStatus.Rejected;
            pass.Remarks = dto?.Remarks;
            pass.ApprovedBy = User.FindFirstValue(ClaimTypes.Name) ?? "Admin";
            pass.ApprovalDate = SchoolERP.Api.Utils.TimeUtils.GetIstTime();
            pass.IsActive = false;

            await _context.SaveChangesAsync();
            return NoContent();
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
            if (!pass.IsActive || pass.Status != GatePassStatus.Approved) return BadRequest("Gate Pass is not valid or active");
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
            pass.Status = GatePassStatus.Expired;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
