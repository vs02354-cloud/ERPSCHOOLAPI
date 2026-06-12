using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Api.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Author { get; set; } = string.Empty;

        public string ISBN { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;

        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }

        public DateTime AddedDate { get; set; } = SchoolERP.Api.Utils.TimeUtils.GetIstTime();
    }
}
