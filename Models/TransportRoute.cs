using System.ComponentModel.DataAnnotations;

namespace SchoolERP.Api.Models
{
    public class TransportRoute
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string RouteName { get; set; } = string.Empty; // e.g., North City Route

        public string StartPoint { get; set; } = string.Empty;
        public string EndPoint { get; set; } = string.Empty;

        public int VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }

        public decimal RouteFare { get; set; }
    }
}
