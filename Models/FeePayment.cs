using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Api.Models
{
    public class FeePayment
    {
        [Key]
        public int Id { get; set; }

        public int StudentId { get; set; }
        public Student? Student { get; set; }

        [Required]
        public string ReceiptNumber { get; set; } = string.Empty;

        public decimal AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [Required]
        public string PaymentMode { get; set; } = string.Empty; // Cash, UPI, Card, NetBanking

        public string Remarks { get; set; } = string.Empty;
    }
}
