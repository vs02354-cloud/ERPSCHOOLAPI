using System;
using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Api.Models
{
    public class EmailDeliveryLog
    {
        [Key]
        public int Id { get; set; }

        public int? StudentId { get; set; }
        public Student? Student { get; set; }

        [Required]
        public string EmailAddress { get; set; } = string.Empty;

        [Required]
        public string MessageContent { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Pending"; // Pending, Delivered, Failed

        public string? ErrorMessage { get; set; }

        public DateTime SentAt { get; set; } = SchoolERP.Api.Utils.TimeUtils.GetIstTime();
    }
}
