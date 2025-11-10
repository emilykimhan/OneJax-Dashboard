// Purpose: Represents one of your four main goals.
// Properties: GoalName, StatusColor, ProgressPercent (for display / styling).
// Usage: Used for tabs and goal filtering in the dashboard.

using StrategicDashboard.Models;

namespace OneJax.StrategicDashboard.Models
{
    public class StrategicGoal
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Strategy> Strategies { get; set; } = new();
    }
}