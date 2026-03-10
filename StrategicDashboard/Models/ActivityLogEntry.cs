namespace OneJaxDashboard.Models
{
    //Talijah 
    public class ActivityLogEntry
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string User { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? Entity { get; set; }
        public string? Details { get; set; }
    }
}
