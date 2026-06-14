using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Api.Models
{
    public class TransportRoute
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string RouteName { get; set; } = string.Empty; // e.g., North City Route

        public string RouteCode { get; set; } = string.Empty;

        public string StartPoint { get; set; } = string.Empty;
        public string EndPoint { get; set; } = string.Empty;

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public bool IsActive { get; set; } = true;

        public decimal RouteFare { get; set; }

        // Navigation
        public ICollection<TransportRouteStop> RouteStops { get; set; } = new List<TransportRouteStop>();
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}
