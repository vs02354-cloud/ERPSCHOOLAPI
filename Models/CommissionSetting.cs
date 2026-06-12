using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Api.Models
{
    public class CommissionSetting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string CommissionType { get; set; } = "Percentage"; // "Fixed" or "Percentage"

        public decimal CommissionValue { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
