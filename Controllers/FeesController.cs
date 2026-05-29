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
    public class FeesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FeesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- Fee Structures ---

        [HttpGet("Structure")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Accountant,Principal,Teacher")]
        public async Task<ActionResult<IEnumerable<FeeStructure>>> GetAllFeeStructures()
        {
            return await _context.FeeStructures.OrderByDescending(f => f.AcademicYear).ThenBy(f => f.ClassName).ToListAsync();
        }

        [HttpPost("Structure")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Accountant,Principal,Teacher")]
        public async Task<ActionResult<FeeStructure>> CreateFeeStructure(FeeStructure structure)
        {
            // Prevent duplicate
            var existing = await _context.FeeStructures.FirstOrDefaultAsync(f => f.ClassName == structure.ClassName && f.AcademicYear == structure.AcademicYear);
            if (existing != null)
            {
                return BadRequest(new { Message = $"Fee structure for {structure.ClassName} ({structure.AcademicYear}) already exists." });
            }

            _context.FeeStructures.Add(structure);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetFeeStructure), new { id = structure.Id }, structure);
        }

        [HttpPut("Structure/{id}")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Accountant,Principal,Teacher")]
        public async Task<IActionResult> UpdateFeeStructure(int id, FeeStructure structure)
        {
            if (id != structure.Id) return BadRequest();

            // Prevent duplicate
            var existing = await _context.FeeStructures.FirstOrDefaultAsync(f => f.ClassName == structure.ClassName && f.AcademicYear == structure.AcademicYear && f.Id != id);
            if (existing != null)
            {
                return BadRequest(new { Message = $"Fee structure for {structure.ClassName} ({structure.AcademicYear}) already exists." });
            }

            _context.Entry(structure).State = EntityState.Modified;
            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FeeStructureExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("Structure/{id}")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Accountant,Principal,Teacher")]
        public async Task<IActionResult> DeleteFeeStructure(int id)
        {
            var structure = await _context.FeeStructures.FindAsync(id);
            if (structure == null) return NotFound();

            _context.FeeStructures.Remove(structure);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("Structure/{id}")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Accountant,Principal,Teacher")]
        public async Task<ActionResult<FeeStructure>> GetFeeStructure(int id)
        {
            var structure = await _context.FeeStructures.FindAsync(id);
            if (structure == null) return NotFound();
            return structure;
        }

        [HttpGet("Structure/Class/{className}")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Accountant,Principal,Teacher")]
        public async Task<ActionResult<IEnumerable<FeeStructure>>> GetStructuresByClass(string className)
        {
            return await _context.FeeStructures.Where(f => f.ClassName == className).ToListAsync();
        }

        private bool FeeStructureExists(int id)
        {
            return _context.FeeStructures.Any(e => e.Id == id);
        }

        // --- Fee Payments ---

        [HttpGet("Payment")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Accountant,Principal")]
        public async Task<ActionResult<IEnumerable<FeePayment>>> GetAllPayments()
        {
            return await _context.FeePayments
                .Include(p => p.Student)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        [HttpPost("Payment")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Accountant,Principal")]
        public async Task<ActionResult<FeePayment>> CollectFee(FeePayment payment)
        {
            // Validate amount against pending fee
            var student = await _context.Students.FindAsync(payment.StudentId);
            if (student == null) return NotFound(new { Message = "Student not found" });

            // Hardcoding "2026-2027" as the current academic year for validation, or we could pass it.
            // Let's assume the frontend passes the expected validation or we just accept the payment if valid structure exists.
            var feeStruct = await _context.FeeStructures
                .Where(f => f.ClassName == student.CurrentClass)
                .OrderByDescending(f => f.AcademicYear) // latest
                .FirstOrDefaultAsync();

            if (feeStruct != null)
            {
                var totalPaid = await _context.FeePayments.Where(p => p.StudentId == payment.StudentId).SumAsync(p => p.AmountPaid);
                var pending = feeStruct.TotalFee - totalPaid;

                if (payment.AmountPaid > pending)
                {
                    return BadRequest(new { Message = $"Payment amount ({payment.AmountPaid}) exceeds pending fee ({pending})." });
                }
            }

            payment.PaymentDate = DateTime.UtcNow;
            
            // Generate simple receipt number
            var count = await _context.FeePayments.CountAsync();
            payment.ReceiptNumber = $"REC{DateTime.Now.Year}{(count + 1):D5}";

            _context.FeePayments.Add(payment);
            await _context.SaveChangesAsync();
            return Ok(payment);
        }

        [HttpPut("Payment/{id}")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Accountant,Principal")]
        public async Task<IActionResult> UpdateFeePayment(int id, FeePayment payment)
        {
            if (id != payment.Id) return BadRequest();

            _context.Entry(payment).State = EntityState.Modified;
            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FeePaymentExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("Payment/{id}")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Accountant,Principal")]
        public async Task<IActionResult> DeleteFeePayment(int id)
        {
            var payment = await _context.FeePayments.FindAsync(id);
            if (payment == null) return NotFound();

            _context.FeePayments.Remove(payment);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("Payment/Student/{studentId}")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Accountant,Principal,Teacher")]
        public async Task<ActionResult<IEnumerable<FeePayment>>> GetStudentPayments(int studentId)
        {
            return await _context.FeePayments
                .Include(p => p.Student)
                .Where(p => p.StudentId == studentId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        [HttpGet("DashboardStats")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Accountant,Principal")]
        public async Task<ActionResult<object>> GetDashboardStats()
        {
            var today = DateTime.UtcNow.Date;
            
            var totalCollection = await _context.FeePayments.SumAsync(p => p.AmountPaid);
            var todayCollection = await _context.FeePayments
                .Where(p => p.PaymentDate.Date == today)
                .SumAsync(p => p.AmountPaid);
            
            var totalStudentsPaid = await _context.FeePayments
                .Select(p => p.StudentId)
                .Distinct()
                .CountAsync();

            // Rough calculation of total pending fees across school (assuming latest fee structure applies to each student)
            var students = await _context.Students.ToListAsync();
            var feeStructures = await _context.FeeStructures.ToListAsync();
            var payments = await _context.FeePayments.ToListAsync();

            decimal totalPendingFees = 0;
            foreach(var s in students)
            {
                var fStruct = feeStructures.Where(f => f.ClassName == s.CurrentClass).OrderByDescending(f => f.AcademicYear).FirstOrDefault();
                if (fStruct != null)
                {
                    var paid = payments.Where(p => p.StudentId == s.Id).Sum(p => p.AmountPaid);
                    totalPendingFees += (fStruct.TotalFee - paid);
                }
            }

            return new { TotalCollection = totalCollection, TodayCollection = todayCollection, PendingFees = totalPendingFees, TotalStudentsPaid = totalStudentsPaid };
        }

        [HttpGet("Trends")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Accountant,Principal")]
        public async Task<ActionResult<IEnumerable<object>>> GetTrends()
        {
            var trends = new List<object>();
            var payments = await _context.FeePayments.ToListAsync();
            var attendances = await _context.Attendances.ToListAsync();
            
            for (int i = 5; i >= 0; i--)
            {
                var targetDate = DateTime.UtcNow.AddMonths(-i);
                
                var monthPayments = payments.Where(p => p.PaymentDate.Month == targetDate.Month && p.PaymentDate.Year == targetDate.Year);
                var revenue = monthPayments.Sum(p => p.AmountPaid);

                var monthAttendances = attendances.Where(a => a.Date.Month == targetDate.Month && a.Date.Year == targetDate.Year);
                int totalAtt = monthAttendances.Count();
                int presentAtt = monthAttendances.Count(a => a.Status == "Present");
                
                int attendancePercent = totalAtt > 0 ? (int)Math.Round((double)presentAtt / totalAtt * 100) : 0;

                trends.Add(new {
                    Month = targetDate.ToString("MMM"),
                    Revenue = revenue,
                    Attendance = attendancePercent
                });
            }

            return Ok(trends);
        }

        [HttpGet("PendingReport")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Accountant,Principal")]
        public async Task<ActionResult<IEnumerable<object>>> GetPendingReport()
        {
            var students = await _context.Students.ToListAsync();
            var feeStructures = await _context.FeeStructures.ToListAsync();
            var payments = await _context.FeePayments.ToListAsync();

            var report = new List<object>();

            foreach(var s in students)
            {
                var fStruct = feeStructures.Where(f => f.ClassName == s.CurrentClass).OrderByDescending(f => f.AcademicYear).FirstOrDefault();
                decimal totalFee = fStruct?.TotalFee ?? 0;
                decimal paid = payments.Where(p => p.StudentId == s.Id).Sum(p => p.AmountPaid);
                decimal pending = totalFee - paid;
                if (pending < 0) pending = 0;

                string status = pending == 0 && totalFee > 0 ? "Paid" : (paid > 0 && pending > 0 ? "Partially Paid" : (totalFee > 0 ? "Pending" : "N/A"));

                report.Add(new {
                    StudentId = s.Id,
                    StudentName = s.FirstName + " " + s.LastName,
                    AdmissionNumber = s.AdmissionNumber,
                    Class = s.CurrentClass,
                    TotalFee = totalFee,
                    PaidFee = paid,
                    PendingFee = pending,
                    Status = status
                });
            }

            return Ok(report);
        }

        [HttpGet("Pending/Student/{studentId}")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Accountant,Principal")]
        public async Task<ActionResult<object>> GetStudentPendingFee(int studentId)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null) return NotFound();

            var fStruct = await _context.FeeStructures.Where(f => f.ClassName == student.CurrentClass).OrderByDescending(f => f.AcademicYear).FirstOrDefaultAsync();
            decimal totalFee = fStruct?.TotalFee ?? 0;
            decimal paid = await _context.FeePayments.Where(p => p.StudentId == studentId).SumAsync(p => p.AmountPaid);
            decimal pending = totalFee - paid;
            if (pending < 0) pending = 0;

            string status = pending == 0 && totalFee > 0 ? "Paid" : (paid > 0 && pending > 0 ? "Partially Paid" : (totalFee > 0 ? "Pending" : "N/A"));

            return new {
                TotalFee = totalFee,
                PaidFee = paid,
                PendingFee = pending,
                Status = status
            };
        }

        private bool FeePaymentExists(int id)
        {
            return _context.FeePayments.Any(e => e.Id == id);
        }
    }
}
