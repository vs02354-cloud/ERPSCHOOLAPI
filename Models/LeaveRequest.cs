using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Api.Models
{
    public class LeaveRequest
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        [Required]
        public string Reason { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        public string ManagerRemarks { get; set; } = string.Empty;
    }
}
