using Microsoft.AspNetCore.Identity;

namespace SchoolERP.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        
        // This can help differentiate user types (e.g., Student, Teacher, Admin, Parent) quickly
        public string UserType { get; set; } = "Student"; 
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
