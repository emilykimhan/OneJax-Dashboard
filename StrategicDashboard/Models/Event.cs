// Purpose: Represents an event in the dashboard
// Usage: Events are organized under strategic goals for categorization

namespace OneJax.StrategicDashboard.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public DateTime Date { get; set; }
        public string Type { get; set; } = ""; // Workshop, Meeting, Training, etc.
        public string Location { get; set; } = "";
        public int StrategicGoalId { get; set; } // Which goal this event supports
        public string Status { get; set; } = "Planned"; // Planned, Active, Completed, Cancelled
        
        // Assessment data
        public string PreAssessmentData { get; set; } = "";
        public string PostAssessmentData { get; set; } = "";
        
        // Optional metrics
        public int Attendees { get; set; }
        public decimal? SatisfactionScore { get; set; }
        public string Notes { get; set; } = "";
    }
}
