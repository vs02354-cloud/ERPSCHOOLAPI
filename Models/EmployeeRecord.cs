using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Api.Models
{
    public class EmployeeRecord
    {
        [Key]
        public int Id { get; set; }

        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser? ApplicationUser { get; set; }

        [Required]
        public string EmployeeIdNumber { get; set; } = string.Empty;

        [Required]
        public string Designation { get; set; } = string.Empty;
        
        public string Department { get; set; } = string.Empty;
        public DateTime JoiningDate { get; set; }

        [Required]
        public decimal BasicSalary { get; set; }
        public string BankAccountNumber { get; set; } = string.Empty;
        public string IFSC_Code { get; set; } = string.Empty;
    }
}
