// Purpose: Represents a specific strategy or action under a goal.
// Properties: Id, Name, GoalName (which goal it belongs to).
// Usage: Displayed and managed in the strategies view for each goal.

using OneJax.StrategicDashboard.Models;
using StrategicDashboard.Models;

public class Strategy
{
    public int Id { get; set; }
    public string Name { get; set; } 
    public int StrategicGoalId { get; set; } 
    public string Description { get; set; }
    public List<Metric> Metrics { get; set; } = new(); 
    public string? Date { get; set; }  
    public string? Time { get; set; }

    public StrategicGoal StrategicGoal { get; set; }
}