// Purpose: Combines all the data needed for your dashboard view.
// Contains: Lists of StrategicGoal and Project (and possibly other dashboard data).
//Usage: Passed from your controller to the dashboard view.


using OneJax.StrategicDashboard.Models; // Add the correct namespace for StrategicGoal

public class DashboardViewModel
{
    public IEnumerable<StrategicGoal> StrategicGoals { get; set; }
    public IEnumerable<Project> Projects { get; set; }
    // other properties

}