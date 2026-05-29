using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Api.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        // Basic Information
        [Required]
        public string EmployeeCode { get; set; } = string.Empty;
        
        [Required]
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        
        public string? LastName { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? MaritalStatus { get; set; }

        // Contact Information
        public string? MobileNumber { get; set; }
        public string? AlternateMobileNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Pincode { get; set; }

        // Official Information
        public string? Designation { get; set; }
        public string? EmployeeType { get; set; } // Teaching / Non-Teaching
        public DateTime? JoiningDate { get; set; }
        public decimal? ExperienceYears { get; set; }
        public string? Qualification { get; set; }

        // Salary Information
        public decimal? BasicSalary { get; set; }
        public string? BankName { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? IFSCCode { get; set; }
        public string? PANNumber { get; set; }
        public string? AadhaarNumber { get; set; }

        // Attendance & Login Link
        public string? Username { get; set; }

        // Emergency Details
        public string? EmergencyContactName { get; set; }
        public string? BloodGroup { get; set; }

        // Document Information
        public string? PhotoPath { get; set; }
        public string? ResumePath { get; set; }
        public string? IDProofPath { get; set; }

        // Audit Fields 
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
