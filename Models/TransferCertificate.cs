using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolERP.Api.Models
{
    public class TransferCertificate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        public string? TCNumber { get; set; }

        public DateTime? IssueDate { get; set; }

        [Required]
        public string ReasonForLeaving { get; set; } = string.Empty;

        public string? AcademicProgress { get; set; }
        
        public string? Conduct { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime AppliedDate { get; set; } = SchoolERP.Api.Utils.TimeUtils.GetIstTime();

        public DateTime CreatedDate { get; set; } = SchoolERP.Api.Utils.TimeUtils.GetIstTime();
    }
}
