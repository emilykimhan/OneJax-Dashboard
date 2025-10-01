// Purpose: Combines all the data needed for your dashboard view.
// Contains: Lists of StrategicGoal and Project (and possibly other dashboard data).
//Usage: Passed from your controller to the dashboard view.


using OneJax.StrategicDashboard.Models;

public class DashboardViewModel
{
    public IEnumerable<StrategicGoal> StrategicGoals { get; set; }
    // Add other dashboard properties as needed
}