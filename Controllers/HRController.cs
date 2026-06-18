using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Api.Data;
using SchoolERP.Api.Models;

using Microsoft.AspNetCore.Identity;
using SchoolERP.Api.Models.DTOs;
using System.Security.Claims;

namespace SchoolERP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HRController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HRController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // --- Employee Records ---

        [HttpGet("Employees")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Principal")]
        public async Task<ActionResult<PaginatedResponse<EmployeeDto>>> GetEmployees(
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            var query = _context.Employees.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var lowerTerm = searchTerm.ToLower();
                query = query.Where(e => 
                    (e.FirstName != null && e.FirstName.ToLower().Contains(lowerTerm)) ||
                    (e.LastName != null && e.LastName.ToLower().Contains(lowerTerm)) ||
                    (e.EmployeeCode != null && e.EmployeeCode.ToLower().Contains(lowerTerm)) ||
                    (e.Designation != null && e.Designation.ToLower().Contains(lowerTerm)) ||
                    (e.EmployeeType != null && e.EmployeeType.ToLower().Contains(lowerTerm)));
            }

            var totalCount = await query.CountAsync();
            
            var employees = await query
                .OrderByDescending(e => e.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = new List<EmployeeDto>();
            foreach (var emp in employees)
            {
                var dto = new EmployeeDto { Employee = emp, IsActive = false };
                if (!string.IsNullOrEmpty(emp.Username))
                {
                    var user = await _userManager.FindByEmailAsync(emp.Username) ?? await _userManager.FindByNameAsync(emp.Username);
                    if (user != null)
                    {
                        dto.IsActive = user.IsActive;
                    }
                }
                dtos.Add(dto);
            }

            return Ok(new PaginatedResponse<EmployeeDto>
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Items = dtos
            });
        }

        [HttpPost("Employee")]
        [Authorize(Roles = "Admin,Super Admin,School Admin")]
        public async Task<ActionResult<Employee>> CreateEmployeeRecord(Employee record)
        {
            _context.Employees.Add(record);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEmployeeRecord), new { id = record.Id }, record);
        }

        [HttpGet("Employee/{id}")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Principal")]
        public async Task<ActionResult<EmployeeDto>> GetEmployeeRecord(int id)
        {
            var record = await _context.Employees.FindAsync(id);
            if (record == null) return NotFound();

            var dto = new EmployeeDto { Employee = record, IsActive = false };
            if (!string.IsNullOrEmpty(record.Username))
            {
                var user = await _userManager.FindByEmailAsync(record.Username) ?? await _userManager.FindByNameAsync(record.Username);
                if (user != null)
                {
                    dto.IsActive = user.IsActive;
                }
            }
            return dto;
        }

        [HttpPut("Employee/{id}")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Principal")]
        public async Task<IActionResult> UpdateEmployeeRecord(int id, [FromBody] EmployeeDto dto)
        {
            if (id != dto.Employee.Id) return BadRequest();

            var existing = await _context.Employees.FindAsync(id);
            if (existing == null) return NotFound();

            // Update Employee properties
            _context.Entry(existing).CurrentValues.SetValues(dto.Employee);
            existing.UpdatedDate = SchoolERP.Api.Utils.TimeUtils.GetIstTime();

            // Attempt to get user id to set UpdatedBy
            var claimsUserId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            // Ignore parse errors, just leave null if missing/not int
            
            await _context.SaveChangesAsync();

            // Update Identity User IsActive status
            if (!string.IsNullOrEmpty(existing.Username))
            {
                var user = await _userManager.FindByEmailAsync(existing.Username) ?? await _userManager.FindByNameAsync(existing.Username);
                if (user != null)
                {
                    if (user.IsActive != dto.IsActive)
                    {
                        user.IsActive = dto.IsActive;
                        var result = await _userManager.UpdateAsync(user);
                        if (!result.Succeeded)
                        {
                            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Failed to update user status.", Errors = result.Errors });
                        }
                    }
                }
                else
                {
                    return BadRequest(new { Message = "User account linked to this employee was not found." });
                }
            }
            else if (dto.IsActive)
            {
                return BadRequest(new { Message = "Cannot activate employee: No user account is linked to this employee." });
            }

            return NoContent();
        }

        [HttpDelete("Employee/{id}")]
        [Authorize(Roles = "Admin,Super Admin,School Admin")]
        public async Task<IActionResult> DeleteEmployeeRecord(int id)
        {
            var record = await _context.Employees.FindAsync(id);
            if (record == null) return NotFound();

            _context.Employees.Remove(record);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("Teachers")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Principal,Receptionist")]
        public async Task<ActionResult<IEnumerable<object>>> GetTeachers()
        {
            var teachers = await _context.Employees
                .Where(e => e.EmployeeType == "Teaching" || e.Designation == "Teacher" || e.Designation == "Teaching")
                .Select(e => new { e.Id, Name = e.FirstName + " " + (e.LastName ?? ""), e.EmployeeCode })
                .ToListAsync();
            return Ok(teachers);
        }

        // --- Leave Requests ---

        [HttpPost("Leave")]
        public async Task<ActionResult<LeaveRequest>> RequestLeave(LeaveRequest request)
        {
            _context.LeaveRequests.Add(request);
            await _context.SaveChangesAsync();
            return Ok(request);
        }

        [HttpPut("Leave/{id}/Approve")]
        [Authorize(Roles = "Admin,Super Admin,School Admin")]
        public async Task<IActionResult> ApproveLeave(int id, [FromBody] string remarks)
        {
            var leave = await _context.LeaveRequests.FindAsync(id);
            if (leave == null) return NotFound();

            leave.Status = "Approved";
            leave.ManagerRemarks = remarks;
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Leave approved" });
        }

        // --- Salary Slips ---

        [HttpPost("Salary")]
        [Authorize(Roles = "Admin,Super Admin,School Admin,Accountant")]
        public async Task<ActionResult<SalarySlip>> GenerateSalarySlip(SalarySlip slip)
        {
            slip.GeneratedDate = SchoolERP.Api.Utils.TimeUtils.GetIstTime();
            _context.SalarySlips.Add(slip);
            await _context.SaveChangesAsync();
            return Ok(slip);
        }

        [HttpGet("Salary/Employee/{employeeId}")]
        public async Task<ActionResult<IEnumerable<SalarySlip>>> GetEmployeeSalarySlips(int employeeId)
        {
            return await _context.SalarySlips
                .Where(s => s.EmployeeId == employeeId)
                .OrderByDescending(s => s.GeneratedDate)
                .ToListAsync();
        }
    }
}
