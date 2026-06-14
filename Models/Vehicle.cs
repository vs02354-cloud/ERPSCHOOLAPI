using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Api.Models
{
    public class Vehicle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string VehicleNumber { get; set; } = string.Empty; // e.g., AB-12-XY-3456

        public string VehicleModel { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty; // Bus/Van/Mini Bus
        public int Capacity { get; set; }
        
        public string RegistrationNumber { get; set; } = string.Empty;
        public DateTime? InsuranceExpiry { get; set; }
        public DateTime? FitnessExpiry { get; set; }
        public string PermitDetails { get; set; } = string.Empty;
        public DateTime? PollutionExpiry { get; set; }

        public string? VehicleImage { get; set; }

        // Assignments
        public int? DriverEmployeeId { get; set; }
        public Employee? Driver { get; set; }

        public int? AttendantEmployeeId { get; set; }
        public Employee? Attendant { get; set; }

        public int? AssignedRouteId { get; set; }
        public TransportRoute? AssignedRoute { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
