using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolERP.Api.Models
{
    public class AssignmentMaster
    {
        [Key]
        public int AssignmentId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string ClassName { get; set; } = string.Empty;

        public string Section { get; set; } = string.Empty;

        [Required]
        public DateTime DueDate { get; set; }

        public int MaxMarks { get; set; }

        [MaxLength(500)]
        public string? AttachmentPath { get; set; }

        public string CreatedBy { get; set; } = string.Empty;
        public ApplicationUser? Creator { get; set; }

        public DateTime CreatedDate { get; set; } = SchoolERP.Api.Utils.TimeUtils.GetIstTime();

        public ICollection<AssignmentSubmission>? Submissions { get; set; }
    }
}
