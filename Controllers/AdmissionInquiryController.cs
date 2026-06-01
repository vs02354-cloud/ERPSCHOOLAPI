using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Api.Data;
using SchoolERP.Api.Models;
using SchoolERP.Api.Models.DTOs;
using System.Security.Claims;

namespace SchoolERP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdmissionInquiryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdmissionInquiryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/AdmissionInquiry (Public Endpoint)
        [HttpPost]
        public async Task<IActionResult> SubmitInquiry([FromBody] CreateAdmissionInquiryDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var today = DateTime.UtcNow.Date;
            var dailyCount = await _context.AdmissionInquiries.CountAsync(i => i.InquiryDate.Date == today);
            var inquiryNo = $"INQ-{today:yyyyMMdd}-{(dailyCount + 1):D3}";

            var inquiry = new AdmissionInquiry
            {
                InquiryNo = inquiryNo,
                InquiryDate = DateTime.UtcNow,
                StudentName = dto.StudentName,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                ClassApplyingFor = dto.ClassApplyingFor,
                ParentName = dto.ParentName,
                MobileNo = dto.MobileNo,
                AlternateMobileNo = dto.AlternateMobileNo,
                EmailId = dto.EmailId,
                CurrentSchool = dto.CurrentSchool,
                Address = dto.Address,
                City = dto.City,
                StateName = dto.StateName,
                Pincode = dto.Pincode,
                InquirySource = dto.InquirySource,
                Remarks = dto.Remarks,
                InquiryStatus = "New",
                CreatedDate = DateTime.UtcNow
            };

            // If an Admin/User is logged in and creating it on behalf of someone, record their ID.
            if (User.Identity?.IsAuthenticated == true)
            {
                // This assumes the user ID can be parsed to an INT (which might fail if Identity uses GUIDs). 
                // However, since the schema specifically requires INT, we'll ignore it for now or try to parse if needed.
                // Assuming public form submission, CreatedBy is null.
            }

            _context.AdmissionInquiries.Add(inquiry);
            await _context.SaveChangesAsync();

            return Ok(new { Status = "Success", Message = "Admission inquiry submitted successfully.", InquiryNumber = inquiryNo });
        }

        // GET: api/AdmissionInquiry
        [HttpGet]
        [Authorize(Roles = "Admin,ADMIN,admin,Principal,PRINCIPAL,principal,Operator,OPERATOR,operator,Receptionist,RECEPTIONIST,receptionist,Super Admin,SUPER ADMIN,super admin,School Admin,SCHOOL ADMIN,school admin")]
        public async Task<ActionResult<IEnumerable<AdmissionInquiry>>> GetInquiries([FromQuery] string? status, [FromQuery] string? search)
        {
            var query = _context.AdmissionInquiries.AsQueryable();

            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                query = query.Where(i => i.InquiryStatus == status);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(i => i.StudentName.Contains(search) || 
                                         i.ParentName.Contains(search) || 
                                         i.MobileNo.Contains(search) || 
                                         i.InquiryNo.Contains(search));
            }

            var list = await query.OrderByDescending(i => i.InquiryDate).ToListAsync();
            return Ok(list);
        }

        // GET: api/AdmissionInquiry/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,ADMIN,admin,Principal,PRINCIPAL,principal,Operator,OPERATOR,operator,Receptionist,RECEPTIONIST,receptionist,Super Admin,SUPER ADMIN,super admin,School Admin,SCHOOL ADMIN,school admin")]
        public async Task<ActionResult<AdmissionInquiry>> GetInquiry(int id)
        {
            var inquiry = await _context.AdmissionInquiries.FindAsync(id);

            if (inquiry == null)
            {
                return NotFound();
            }

            return Ok(inquiry);
        }

        // PUT: api/AdmissionInquiry/5/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,ADMIN,admin,Principal,PRINCIPAL,principal,Operator,OPERATOR,operator,Receptionist,RECEPTIONIST,receptionist,Super Admin,SUPER ADMIN,super admin,School Admin,SCHOOL ADMIN,school admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateInquiryStatusDto dto)
        {
            var inquiry = await _context.AdmissionInquiries.FindAsync(id);

            if (inquiry == null)
            {
                return NotFound();
            }

            inquiry.InquiryStatus = dto.Status;
            
            if (dto.FollowUpDate.HasValue)
            {
                inquiry.FollowUpDate = dto.FollowUpDate.Value;
            }

            if (!string.IsNullOrEmpty(dto.Remarks))
            {
                // Append remarks with date
                var newRemark = $"[{DateTime.UtcNow:yyyy-MM-dd}] {dto.Remarks}";
                inquiry.Remarks = string.IsNullOrEmpty(inquiry.Remarks) ? newRemark : $"{inquiry.Remarks}\n{newRemark}";
            }

            inquiry.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { Status = "Success", Message = "Inquiry status updated successfully." });
        }

        // GET: api/AdmissionInquiry/summary
        [HttpGet("summary")]
        [Authorize(Roles = "Admin,ADMIN,admin,Principal,PRINCIPAL,principal,Operator,OPERATOR,operator,Receptionist,RECEPTIONIST,receptionist,Super Admin,SUPER ADMIN,super admin,School Admin,SCHOOL ADMIN,school admin")]
        public async Task<IActionResult> GetSummary()
        {
            var summary = await _context.AdmissionInquiries
                .GroupBy(i => i.InquiryStatus)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var total = await _context.AdmissionInquiries.CountAsync();

            return Ok(new { Total = total, Details = summary });
        }
    }
}
