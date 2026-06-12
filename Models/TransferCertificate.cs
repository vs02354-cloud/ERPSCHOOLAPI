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

        [Required]
        public string TCNumber { get; set; } = string.Empty;

        [Required]
        public DateTime IssueDate { get; set; } = SchoolERP.Api.Utils.TimeUtils.GetIstTime();

        [Required]
        public string ReasonForLeaving { get; set; } = string.Empty;

        public string AcademicProgress { get; set; } = string.Empty;
        
        public string Conduct { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = SchoolERP.Api.Utils.TimeUtils.GetIstTime();
    }
}
