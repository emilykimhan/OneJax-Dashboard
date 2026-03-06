// Purpose: Represents a specific strategy or action under a goal.
// Properties: Id, Name, GoalName (which goal it belongs to).
// Usage: Displayed and managed in the strategies view for each goal.

//Dina's
using System.Collections.Generic;

namespace OneJaxDashboard.Models;

public class Strategy
{
    public int Id { get; set; }
    public required string Name { get; set; } 
    public string? ProgramName { get; set; }
    public string? ProgramType { get; set; }
    public int? ProgramId { get; set; }
    public int StrategicGoalId { get; set; } 
    public string Description { get; set; } = string.Empty;
    public string? Date { get; set; }  
    public string? Time { get; set; }
    public string CrossCollaboration { get; set; } = string.Empty;
    public string Partners { get; set; } = string.Empty;
    public string EventFYear { get; set; } = string.Empty;
    public bool IsArchived { get; set; }
    public DateTime? ArchivedAtUtc { get; set; }
    public StrategicGoal? StrategicGoal { get; set; }
    public Programs? Program { get; set; }
}
