using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolERP.Api.Models
{
    public enum GatePassStatus
    {
        Pending,
        Approved,
        Rejected,
        Used,
        Expired
    }

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

        public GatePassStatus Status { get; set; } = GatePassStatus.Approved; // Default to Approved to not break existing ones before migration
        
        public string CreatedBy { get; set; } = "Admin";
        
        public DateTime CreatedDate { get; set; } = SchoolERP.Api.Utils.TimeUtils.GetIstTime();
        
        public string? ApprovedBy { get; set; }
        
        public DateTime? ApprovalDate { get; set; }
        
        public string? Remarks { get; set; }
        
        public int? ValidityPeriodDays { get; set; }
    }
}
