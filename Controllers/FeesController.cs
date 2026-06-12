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
                if (payment.IncludesTransportFee)
                {
                    payment.TransportFeeAmount = feeStruct.TransportFee;
                }
                else
                {
                    payment.TransportFeeAmount = 0;
                }

                decimal academicTotalFee = feeStruct.TotalFee - feeStruct.TransportFee;
                var pastPayments = await _context.FeePayments.Where(p => p.StudentId == payment.StudentId).ToListAsync();
                var totalAcademicPaid = pastPayments.Sum(p => p.AmountPaid - p.TransportFeeAmount);
                var pendingAcademic = academicTotalFee - totalAcademicPaid;

                decimal academicPayingNow = payment.AmountPaid - payment.TransportFeeAmount;

                if (academicPayingNow > pendingAcademic)
                {
                    return BadRequest(new { Message = $"Academic payment amount ({academicPayingNow}) exceeds pending academic fee ({pendingAcademic})." });
                }
                
                if (payment.IncludesTransportFee && payment.AmountPaid < feeStruct.TransportFee)
                {
                    return BadRequest(new { Message = $"Amount paid must cover the transport fee of {feeStruct.TransportFee}." });
                }
            }

            payment.PaymentDate = SchoolERP.Api.Utils.TimeUtils.GetIstTime();
            
            // Generate simple receipt number
            var count = await _context.FeePayments.CountAsync();
            payment.ReceiptNumber = $"REC{SchoolERP.Api.Utils.TimeUtils.GetIstTime().Year}{(count + 1):D5}";

            _context.FeePayments.Add(payment);
            await _context.SaveChangesAsync();
            
            // Commission Generation
            if (student.ReferredByEmployeeId.HasValue)
            {
                var commissionSetting = await _context.CommissionSettings.FirstOrDefaultAsync(c => c.IsActive);
                if (commissionSetting != null)
                {
                    decimal commissionAmount = 0;
                    if (commissionSetting.CommissionType == "Fixed")
                    {
                        commissionAmount = commissionSetting.CommissionValue;
                    }
                    else if (commissionSetting.CommissionType == "Percentage")
                    {
                        commissionAmount = payment.AmountPaid * (commissionSetting.CommissionValue / 100);
                    }

                    if (commissionAmount > 0)
                    {
                        var commission = new TeacherCommission
                        {
                            EmployeeId = student.ReferredByEmployeeId.Value,
                            StudentId = student.Id,
                            FeePaymentId = payment.Id,
                            CommissionAmount = commissionAmount,
                            DateEarned = SchoolERP.Api.Utils.TimeUtils.GetIstTime(),
                            IsPaid = false
                        };
                        _context.TeacherCommissions.Add(commission);
                        await _context.SaveChangesAsync();
                    }
                }
            }

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
            var today = SchoolERP.Api.Utils.TimeUtils.GetIstTime().Date;
            
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
                    decimal academicTotalFee = fStruct.TotalFee - fStruct.TransportFee;
                    var paid = payments.Where(p => p.StudentId == s.Id).Sum(p => p.AmountPaid - p.TransportFeeAmount);
                    totalPendingFees += (academicTotalFee - paid);
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
                var targetDate = SchoolERP.Api.Utils.TimeUtils.GetIstTime().AddMonths(-i);
                
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
                decimal academicTotalFee = totalFee - (fStruct?.TransportFee ?? 0);
                
                decimal paid = payments.Where(p => p.StudentId == s.Id).Sum(p => p.AmountPaid);
                decimal academicPaid = payments.Where(p => p.StudentId == s.Id).Sum(p => p.AmountPaid - p.TransportFeeAmount);
                
                decimal pending = academicTotalFee - academicPaid;
                if (pending < 0) pending = 0;

                string status = pending == 0 && academicTotalFee > 0 ? "Paid" : (academicPaid > 0 && pending > 0 ? "Partially Paid" : (academicTotalFee > 0 ? "Pending" : "N/A"));

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
            decimal academicTotalFee = totalFee - (fStruct?.TransportFee ?? 0);
            
            var payments = await _context.FeePayments.Where(p => p.StudentId == studentId).ToListAsync();
            decimal paid = payments.Sum(p => p.AmountPaid);
            decimal academicPaid = payments.Sum(p => p.AmountPaid - p.TransportFeeAmount);
            
            decimal pending = academicTotalFee - academicPaid;
            if (pending < 0) pending = 0;

            string status = pending == 0 && academicTotalFee > 0 ? "Paid" : (academicPaid > 0 && pending > 0 ? "Partially Paid" : (academicTotalFee > 0 ? "Pending" : "N/A"));

            return new {
                TotalFee = totalFee,
                PaidFee = paid,
                PendingFee = pending,
                Status = status,
                TransportFee = fStruct?.TransportFee ?? 0
            };
        }

        // --- Parent Specific Endpoints ---

        [HttpGet("Parent/DashboardStats")]
        [Authorize(Roles = "Parent,PARENT,parent")]
        public async Task<ActionResult<object>> GetParentDashboardStats()
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName)) return Unauthorized();

            var childrenIds = await _context.Students
                .Where(s => s.ParentContactNumber == userName && s.IsActive)
                .Select(s => s.Id)
                .ToListAsync();

            if (!childrenIds.Any()) 
            {
                return new { TotalCollection = 0, TodayCollection = 0, PendingFees = 0, TotalStudentsPaid = 0 };
            }

            var today = SchoolERP.Api.Utils.TimeUtils.GetIstTime().Date;
            
            var totalCollection = await _context.FeePayments
                .Where(p => childrenIds.Contains(p.StudentId))
                .SumAsync(p => p.AmountPaid);
                
            var todayCollection = await _context.FeePayments
                .Where(p => childrenIds.Contains(p.StudentId) && p.PaymentDate.Date == today)
                .SumAsync(p => p.AmountPaid);
            
            var totalStudentsPaid = await _context.FeePayments
                .Where(p => childrenIds.Contains(p.StudentId))
                .Select(p => p.StudentId)
                .Distinct()
                .CountAsync();

            var students = await _context.Students.Where(s => childrenIds.Contains(s.Id)).ToListAsync();
            var feeStructures = await _context.FeeStructures.ToListAsync();
            var payments = await _context.FeePayments.Where(p => childrenIds.Contains(p.StudentId)).ToListAsync();

            decimal totalPendingFees = 0;
            foreach(var s in students)
            {
                var fStruct = feeStructures.Where(f => f.ClassName == s.CurrentClass).OrderByDescending(f => f.AcademicYear).FirstOrDefault();
                if (fStruct != null)
                {
                    decimal academicTotalFee = fStruct.TotalFee - fStruct.TransportFee;
                    var paid = payments.Where(p => p.StudentId == s.Id).Sum(p => p.AmountPaid - p.TransportFeeAmount);
                    totalPendingFees += (academicTotalFee - paid);
                }
            }

            return new { TotalCollection = totalCollection, TodayCollection = todayCollection, PendingFees = totalPendingFees, TotalStudentsPaid = totalStudentsPaid };
        }

        [HttpGet("Parent/PendingReport")]
        [Authorize(Roles = "Parent,PARENT,parent")]
        public async Task<ActionResult<IEnumerable<object>>> GetParentPendingReport()
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName)) return Unauthorized();

            var students = await _context.Students
                .Where(s => s.ParentContactNumber == userName && s.IsActive)
                .ToListAsync();

            var feeStructures = await _context.FeeStructures.ToListAsync();
            var childrenIds = students.Select(s => s.Id).ToList();
            var payments = await _context.FeePayments.Where(p => childrenIds.Contains(p.StudentId)).ToListAsync();

            var report = new List<object>();

            foreach(var s in students)
            {
                var fStruct = feeStructures.Where(f => f.ClassName == s.CurrentClass).OrderByDescending(f => f.AcademicYear).FirstOrDefault();
                decimal totalFee = fStruct?.TotalFee ?? 0;
                decimal academicTotalFee = totalFee - (fStruct?.TransportFee ?? 0);
                
                decimal paid = payments.Where(p => p.StudentId == s.Id).Sum(p => p.AmountPaid);
                decimal academicPaid = payments.Where(p => p.StudentId == s.Id).Sum(p => p.AmountPaid - p.TransportFeeAmount);
                
                decimal pending = academicTotalFee - academicPaid;
                if (pending < 0) pending = 0;

                string status = pending == 0 && academicTotalFee > 0 ? "Paid" : (academicPaid > 0 && pending > 0 ? "Partially Paid" : (academicTotalFee > 0 ? "Pending" : "N/A"));

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

        [HttpGet("Parent/Payment")]
        [Authorize(Roles = "Parent,PARENT,parent")]
        public async Task<ActionResult<IEnumerable<FeePayment>>> GetParentPayments()
        {
            var userName = User.Identity?.Name;
            if (string.IsNullOrEmpty(userName)) return Unauthorized();

            var childrenIds = await _context.Students
                .Where(s => s.ParentContactNumber == userName && s.IsActive)
                .Select(s => s.Id)
                .ToListAsync();

            return await _context.FeePayments
                .Include(p => p.Student)
                .Where(p => childrenIds.Contains(p.StudentId))
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        private bool FeePaymentExists(int id)
        {
            return _context.FeePayments.Any(e => e.Id == id);
        }
    }
}
