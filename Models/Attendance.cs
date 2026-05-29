using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Api.Models
{
    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        public int StudentId { get; set; }
        public Student? Student { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string Status { get; set; } = "Present"; // Present, Absent, Late, HalfDay

        public string Remarks { get; set; } = string.Empty;
    }
}
