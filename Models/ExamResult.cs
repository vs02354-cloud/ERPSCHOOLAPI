using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Api.Models
{
    public class ExamResult
    {
        [Key]
        public int Id { get; set; }

        public int ExamId { get; set; }
        public Exam? Exam { get; set; }

        public int StudentId { get; set; }
        public Student? Student { get; set; }

        [Required]
        public string Subject { get; set; } = string.Empty;

        public decimal MarksObtained { get; set; }
        public decimal TotalMarks { get; set; }
        
        public string Grade { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
    }
}
