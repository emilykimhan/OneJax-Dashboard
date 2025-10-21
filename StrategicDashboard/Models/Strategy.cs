// Purpose: Represents a specific strategy or action under a goal.
// Properties: Id, Name, GoalName (which goal it belongs to).
// Usage: Displayed and managed in the strategies view for each goal.

public class Strategy
{
    public int Id { get; set; } 
    public string Name { get; set; } // the name of the strategy
    public int StrategicGoalId { get; set; } // foreign key to the StrategicGoal
    public string Description { get; set; }
    public List<Metric> Metrics { get; set; } = new(); // list of associated metrics
}