using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Api.Models
{
    public class BookIssue
    {
        [Key]
        public int Id { get; set; }

        public int BookId { get; set; }
        public Book? Book { get; set; }

        public string IssuedToUserId { get; set; } = string.Empty; // AspNetUsers Id
        public ApplicationUser? IssuedToUser { get; set; }

        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        public decimal FineAmount { get; set; }
        public string Status { get; set; } = "Issued"; // Issued, Returned, Overdue
    }
}
