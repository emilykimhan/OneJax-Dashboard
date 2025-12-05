// Purpose: Represents one of your four main goals.
// Properties: GoalName, StatusColor, ProgressPercent (for display / styling).
// Usage: Used for tabs and goal filtering in the dashboard.

//emily 
using OneJaxDashboard.Models;

namespace OneJaxDashboard.Models
{
    public class StrategicGoal
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public List<Event> Events { get; set; } = new(); // Changed from Strategies to Events
        public List<GoalMetric> Metrics { get; set; } = new(); // Track KPIs and metrics
        public string Description { get; set; } = "";
        public string Color { get; set; } = ""; // For dashboard styling

        public List<Strategy> Strategies { get; set; } = new();
    }
}