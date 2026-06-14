using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolERP.Api.Models
{
    public class TransportGatePass
    {
        [Key]
        public int Id { get; set; }

        public int StudentId { get; set; }
        public Student? Student { get; set; }

        public int RouteId { get; set; }
        public TransportRoute? Route { get; set; }

        public int VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }

        [Required]
        public string QRCodeData { get; set; } = string.Empty;

        public DateTime ValidUntil { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public DateTime IssueDate { get; set; } = SchoolERP.Api.Utils.TimeUtils.GetIstTime();
    }
}
