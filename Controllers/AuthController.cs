using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SchoolERP.Api.Models;
using SchoolERP.Api.Models.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace SchoolERP.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly Data.ApplicationDbContext _context;

        public AuthController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, Data.ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var username = model.UserType == "Parent" ? model.MobileNumber : model.Email;
            var email = model.UserType == "Parent" ? null : model.Email;

            ApplicationUser? userExists = null;
            if (!string.IsNullOrEmpty(email)) 
            {
                 userExists = await _userManager.FindByEmailAsync(email);
            }
            if (userExists == null && !string.IsNullOrEmpty(username))
            {
                 userExists = await _userManager.FindByNameAsync(username);
            }

            if (userExists != null)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = "User already exists!" });

            ApplicationUser user = new()
            {
                Email = email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = username,
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserType = model.UserType,
                PhoneNumber = model.MobileNumber,
                IsActive = !(model.UserType == "Teacher" || model.UserType == "Operator" || model.UserType == "Principal")
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = errors });
            }

            try
            {
                if (!await _roleManager.RoleExistsAsync(model.UserType))
                    await _roleManager.CreateAsync(new IdentityRole(model.UserType));

                await _userManager.AddToRoleAsync(user, model.UserType);

                // Link Identity to ERP Profiles
                if (model.UserType == "Student")
                {
                    var count = _context.Students.Count();
                    var student = new Student
                    {
                        ApplicationUserId = user.Id,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        AdmissionNumber = $"ADM{SchoolERP.Api.Utils.TimeUtils.GetIstTime().Year}{(count + 1):D4}",
                        AdmissionDate = SchoolERP.Api.Utils.TimeUtils.GetIstTime()
                    };
                    _context.Students.Add(student);
                }
                else if (model.UserType == "Parent")
                {
                    var studentsToLink = _context.Students.Where(s => s.ParentContactNumber == model.MobileNumber).ToList();
                    foreach (var s in studentsToLink)
                    {
                        s.ParentUserId = user.Id;
                    }
                }
                else // Teachers, Admins, Principals, etc.
                {
                    var count = _context.Employees.Count();
                    var employee = new Employee
                    {
                        EmployeeCode = $"EMP{SchoolERP.Api.Utils.TimeUtils.GetIstTime().Year}{(count + 1):D4}",
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Username = model.Email,
                        Designation = model.UserType,
                        MobileNumber = model.MobileNumber
                    };
                    _context.Employees.Add(employee);
                }

                await _context.SaveChangesAsync();
                return Ok(new { Status = "Success", Message = "User created successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = "Post-creation error: " + ex.Message + " Inner: " + ex.InnerException?.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email) ?? await _userManager.FindByNameAsync(model.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                if (!user.IsActive)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new { Status = "Error", Message = "Account pending admin approval." });
                }
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim("UserType", user.UserType),
                    new Claim("FirstName", user.FirstName ?? ""),
                    new Claim("LastName", user.LastName ?? ""),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = GetToken(authClaims);

                if (user.UserType == "Teacher")
                {
                    _context.SystemActivities.Add(new SystemActivity
                    {
                        Text = $"Teacher {user.FirstName} {user.LastName} logged in.",
                        ActivityType = "Login",
                        UserId = user.Id
                    });
                    await _context.SaveChangesAsync();
                }

                return Ok(new AuthResponseDto
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    Email = user.Email!,
                    FirstName = user.FirstName!,
                    LastName = user.LastName!,
                    UserType = user.UserType
                });
            }
            return Unauthorized();
        }

        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecurityKey"]!));

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                expires: SchoolERP.Api.Utils.TimeUtils.GetIstTime().AddMinutes(Convert.ToDouble(jwtSettings["ExpiryInMinutes"])),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            return Ok(new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.PhoneNumber,
                user.UserName,
                user.UserType,
                user.CreatedAt,
                user.IsActive
            });
        }
    }
}
