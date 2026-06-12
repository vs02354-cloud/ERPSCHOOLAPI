using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Api.Models
{
    public class TeacherCommission
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public int StudentId { get; set; }
        public Student? Student { get; set; }

        public int FeePaymentId { get; set; }
        public FeePayment? FeePayment { get; set; }

        public decimal CommissionAmount { get; set; }

        public DateTime DateEarned { get; set; } = DateTime.UtcNow;

        public bool IsPaid { get; set; } = false;
    }
}
