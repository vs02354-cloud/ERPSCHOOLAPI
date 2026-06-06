using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolERP.Api.Models
{
    public class AssignmentSubmission
    {
        [Key]
        public int SubmissionId { get; set; }

        [Required]
        public int AssignmentId { get; set; }
        public AssignmentMaster? Assignment { get; set; }

        [Required]
        public int StudentId { get; set; }
        public Student? Student { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;

        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Submitted, Late Submitted, Evaluated, Rejected

        [Column(TypeName = "decimal(5,2)")]
        public decimal? MarksObtained { get; set; }

        [MaxLength(500)]
        public string? TeacherRemark { get; set; }
    }
}
