using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Api.Models.DTOs
{
    public class CreateAdmissionInquiryDto
    {
        [Required]
        public string StudentName { get; set; } = null!;
        
        public DateTime? DateOfBirth { get; set; }
        
        public string? Gender { get; set; }
        
        [Required]
        public string ClassApplyingFor { get; set; } = null!;
        
        [Required]
        public string ParentName { get; set; } = null!;
        
        [Required]
        public string MobileNo { get; set; } = null!;
        
        public string? AlternateMobileNo { get; set; }
        
        public string? EmailId { get; set; }
        
        public string? CurrentSchool { get; set; }
        
        public string? Address { get; set; }
        
        public string? City { get; set; }
        
        public string? StateName { get; set; }
        
        public string? Pincode { get; set; }
        
        public string? InquirySource { get; set; }
        
        public string? Remarks { get; set; }
    }

    public class UpdateInquiryStatusDto
    {
        [Required]
        public string Status { get; set; } = null!; // New, Contacted, Interested, Admission Confirmed, Not Interested
        
        public DateTime? FollowUpDate { get; set; }
        
        public string? Remarks { get; set; }
    }
}
