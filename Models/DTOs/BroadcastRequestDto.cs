namespace SchoolERP.Api.Models.DTOs
{
    public class BroadcastRequestDto
    {
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string TargetAudience { get; set; } = "All"; // "All" or "SpecificClass"
        public string? TargetClass { get; set; }
    }
}
