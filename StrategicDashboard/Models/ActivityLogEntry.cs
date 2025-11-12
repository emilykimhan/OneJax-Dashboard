namespace StrategicDashboard.Models
{
    public class ActivityLogEntry
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // e.g., Created Project, Edited Profile
        public string? EntityType { get; set; } // Project, Profile
        public int? EntityId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }
    }
}
