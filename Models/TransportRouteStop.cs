using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolERP.Api.Models
{
    public class TransportRouteStop
    {
        [Key]
        public int Id { get; set; }

        public int RouteId { get; set; }
        public TransportRoute? Route { get; set; }

        [Required]
        public string StopName { get; set; } = string.Empty;

        public int Sequence { get; set; }

        public TimeSpan? EstimatedArrivalTime { get; set; }

        public decimal DistanceFromStart { get; set; }

        public decimal StopFare { get; set; } // If stop-wise fee is applied

        public bool IsActive { get; set; } = true;
    }
}
