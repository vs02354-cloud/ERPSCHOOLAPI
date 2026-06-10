using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Api.Models
{
    public class LeaveRequest
    {
        [Key]
        public int Id { get; set; }

        public int? StudentId { get; set; }
        public Student? Student { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        [Required]
        public string Reason { get; set; } = string.Empty;

        public string Status { get; set; } = "approve"; // approve, Rejected
        public string ManagerRemarks { get; set; } = string.Empty;
    }
}
