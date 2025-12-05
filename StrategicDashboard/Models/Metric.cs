
//Emily's
namespace OneJaxDashboard.Models
{
    public class Metric
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
        public string Progress { get; set; } = string.Empty;
        public int StrategyId { get; set; }
        public string Status { get; set; } = string.Empty;      // e.g., "Active", "Completed", "Upcoming"
        public string TimePeriod { get; set; } = string.Empty;  // e.g., "Monthly", "Quarterly", "Yearly"
    }
}