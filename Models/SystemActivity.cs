using System;
using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Api.Models
{
    public class SystemActivity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = Utils.TimeUtils.GetIstTime();

        public string? UserId { get; set; }

        public string? ActivityType { get; set; }
    }
}
