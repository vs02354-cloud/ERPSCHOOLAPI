using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Api.Models
{
    public class Timetable
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ClassName { get; set; } = string.Empty;

        [Required]
        public string Section { get; set; } = string.Empty;

        [Required]
        public string DayOfWeek { get; set; } = string.Empty; // Monday, Tuesday...

        [Required]
        public string Period { get; set; } = string.Empty; // e.g., "1", "2" or time range "09:00 - 09:45"

        [Required]
        public string Subject { get; set; } = string.Empty;

        public string TeacherId { get; set; } = string.Empty; // Link to AspNetUsers
        public ApplicationUser? Teacher { get; set; }
    }
}
