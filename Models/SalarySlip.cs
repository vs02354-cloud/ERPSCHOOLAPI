using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Api.Models
{
    public class SalarySlip
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        public string Month { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;

        public decimal BasicSalary { get; set; }
        public decimal Allowances { get; set; }
        public decimal Deductions { get; set; } // PF, ESI, Tax

        public decimal NetSalary => BasicSalary + Allowances - Deductions;

        public DateTime GeneratedDate { get; set; } = DateTime.UtcNow;
    }
}
