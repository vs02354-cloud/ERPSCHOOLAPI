using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Api.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }
        
        public string? ApplicationUserId { get; set; } // Link to AspNetUsers
        public ApplicationUser? ApplicationUser { get; set; }

        [Required]
        public string AdmissionNumber { get; set; } = string.Empty;
        
        [Required]
        public string FirstName { get; set; } = string.Empty;
        
        public string LastName { get; set; } = string.Empty;
        
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string BloodGroup { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        // Parent Details
        public string? ParentUserId { get; set; } // Link to AspNetUsers for the parent
        public string FatherName { get; set; } = string.Empty;
        public string MotherName { get; set; } = string.Empty;
        public string ParentContactNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        // Academic Details
        public string CurrentClass { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string PreviousSchool { get; set; } = string.Empty;
        
        public bool TransportRequired { get; set; }
        public DateTime AdmissionDate { get; set; } = DateTime.UtcNow;

        // TC tracking
        public bool IsActive { get; set; } = true;

        // Referral
        public int? ReferredByEmployeeId { get; set; }
        public Employee? ReferredByEmployee { get; set; }
    }
}
