using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolERP.Api.Models
{
    public class DriverLocation
    {
        [Key]
        public int Id { get; set; }

        public int VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }

        public int DriverEmployeeId { get; set; }
        public Employee? DriverEmployee { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double SpeedKmH { get; set; }
        
        public DateTime Timestamp { get; set; } = SchoolERP.Api.Utils.TimeUtils.GetIstTime();

        public bool IsActive { get; set; } = true;
    }
}
