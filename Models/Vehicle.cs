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
        public int Capacity { get; set; }
        
        public string DriverName { get; set; } = string.Empty;
        public string DriverContact { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
