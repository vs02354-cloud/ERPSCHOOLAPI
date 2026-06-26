using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolERP.Api.Models
{
    public class StudentPromotion
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int StudentId { get; set; }
        
        [ForeignKey("StudentId")]
        public Student? Student { get; set; }
        
        [Required]
        public string FromClass { get; set; } = string.Empty;
        
        [Required]
        public string ToClass { get; set; } = string.Empty;
        
        [Required]
        public DateTime PromotionDate { get; set; } = SchoolERP.Api.Utils.TimeUtils.GetIstTime();
        
        [Required]
        public string PromotedBy { get; set; } = string.Empty;
    }
}
