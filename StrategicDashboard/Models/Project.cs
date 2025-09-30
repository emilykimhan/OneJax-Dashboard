// Purpose: Represents an individual project or initiative.
// Properties: Name, GoalName(which goal it belongs to), Status, StatusColor, ProgressPercent, TimePeriod, etc.
// Usage: Displayed as cards on your dashboard.


namespace OneJax.StrategicDashboard.Models
{
    public class Project
    {
        public string Name { get; set; }
        public string GoalName { get; set; }
        public string Status { get; set; }
        public string StatusColor { get; set; }
        public int ProgressPercent { get; set; }
        public string TimePeriod { get; set; }
        // Add other properties as needed
    }
}