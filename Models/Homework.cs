using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Api.Models
{
    public class Homework
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ClassName { get; set; } = string.Empty;

        [Required]
        public string Section { get; set; } = string.Empty;

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; }

        public string TeacherId { get; set; } = string.Empty;
        public ApplicationUser? Teacher { get; set; }
    }
}
