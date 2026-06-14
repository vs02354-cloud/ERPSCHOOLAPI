using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolERP.Api.Models
{
    public class TransportAttendance
    {
        [Key]
        public int Id { get; set; }

        public int StudentId { get; set; }
        public Student? Student { get; set; }

        public DateTime Date { get; set; } = SchoolERP.Api.Utils.TimeUtils.GetIstTime().Date;

        public TimeSpan? BoardingTime { get; set; }
        public TimeSpan? DeboardingTime { get; set; }

        public string Status { get; set; } = string.Empty; // e.g., "Boarded", "Deboarded", "Absent"
        
        public int? ScannedByEmployeeId { get; set; }
        public Employee? ScannedByEmployee { get; set; }
        
        public string Method { get; set; } = "QR Scan"; // "QR Scan", "Manual", "RFID"
    }
}
