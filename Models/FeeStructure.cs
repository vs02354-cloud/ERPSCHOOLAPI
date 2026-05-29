using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Api.Models
{
    public class FeeStructure
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ClassName { get; set; } = string.Empty;

        [Required]
        public string AcademicYear { get; set; } = string.Empty;

        public decimal TuitionFee { get; set; }
        public decimal TransportFee { get; set; }
        public decimal LibraryFee { get; set; }
        public decimal MiscellaneousFee { get; set; }

        public decimal TotalFee => TuitionFee + TransportFee + LibraryFee + MiscellaneousFee;
    }
}
