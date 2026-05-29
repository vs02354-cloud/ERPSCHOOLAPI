using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Api.Models
{
    public class Notice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public string TargetAudience { get; set; } = "All"; // All, Student, Teacher, Parent

        public DateTime PublishDate { get; set; } = DateTime.UtcNow;

        public string CreatedById { get; set; } = string.Empty;
        public ApplicationUser? CreatedBy { get; set; }
    }
}
