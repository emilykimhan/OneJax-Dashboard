// Purpose: Represents one of your four main goals.
// Properties: GoalName, StatusColor, ProgressPercent (for display / styling).
// Usage: Used for tabs and goal filtering in the dashboard.

//emily 
using System.ComponentModel.DataAnnotations.Schema;
using OneJaxDashboard.Models;

namespace OneJaxDashboard.Models
{
    public class StrategicGoal
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        [NotMapped]
        public List<Event> Events { get; set; } = new();
        [NotMapped]
        public int ScheduledMetricCount { get; set; }
        [NotMapped]
        public int ReportingMetricCount { get; set; }
        [NotMapped]
        public decimal Progress { get; set; }
        [NotMapped]
        public int OverallScheduledMetricCount { get; set; }
        [NotMapped]
        public int OverallReportingMetricCount { get; set; }
        [NotMapped]
        public decimal OverallProgress { get; set; }
        [NotMapped]
        public int OverallEventCount { get; set; }
        public List<GoalMetric> Metrics { get; set; } = new(); // Track KPIs and metrics
        public string Description { get; set; } = "";
        public string Color { get; set; } = ""; // For dashboard styling

        public List<Strategy> Strategies { get; set; } = new();
    }
}
