// Purpose: Represents one of your four main goals.
// Properties: GoalName, StatusColor, ProgressPercent (for display / styling).
// Usage: Used for tabs and goal filtering in the dashboard.


namespace OneJax.StrategicDashboard.Models
{
    public class StrategicGoal
    {
        public string GoalName { get; set; }
        public string StatusColor { get; set; }
        public int ProgressPercent { get; set; }
        // Add other properties as needed
    }
}