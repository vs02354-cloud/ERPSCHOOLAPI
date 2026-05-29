using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Api.Models
{
    public class Exam
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty; // e.g., MidTerm, Final

        public string ClassName { get; set; } = string.Empty;
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public ICollection<ExamResult>? Results { get; set; }
    }
}
