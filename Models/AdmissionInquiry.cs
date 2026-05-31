using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolERP.Api.Models
{
    [Table("AdmissionInquiry")]
    public class AdmissionInquiry
    {
        [Key]
        public int InquiryId { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string InquiryNo { get; set; } = null!;
        
        public DateTime InquiryDate { get; set; }
        
        [Required]
        [MaxLength(150)]
        public string StudentName { get; set; } = null!;
        
        public DateTime? DateOfBirth { get; set; }
        
        [MaxLength(20)]
        public string? Gender { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string ClassApplyingFor { get; set; } = null!;
        
        [Required]
        [MaxLength(150)]
        public string ParentName { get; set; } = null!;
        
        [Required]
        [MaxLength(15)]
        public string MobileNo { get; set; } = null!;
        
        [MaxLength(15)]
        public string? AlternateMobileNo { get; set; }
        
        [MaxLength(150)]
        public string? EmailId { get; set; }
        
        [MaxLength(200)]
        public string? CurrentSchool { get; set; }
        
        [MaxLength(500)]
        public string? Address { get; set; }
        
        [MaxLength(100)]
        public string? City { get; set; }
        
        [MaxLength(100)]
        public string? StateName { get; set; }
        
        [MaxLength(10)]
        public string? Pincode { get; set; }
        
        [MaxLength(100)]
        public string? InquirySource { get; set; }
        
        public DateTime? FollowUpDate { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string InquiryStatus { get; set; } = "New";
        
        public string? Remarks { get; set; }
        
        public int? CreatedBy { get; set; }
        
        public DateTime CreatedDate { get; set; }
        
        public int? ModifiedBy { get; set; }
        
        public DateTime? ModifiedDate { get; set; }
    }
}
