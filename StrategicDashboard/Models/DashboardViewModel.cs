// Purpose: Combines all the data needed for your dashboard view.
// Contains: Lists of StrategicGoal and Project (and possibly other dashboard data).
//Usage: Passed from your controller to the dashboard view.

//Emily
using OneJaxDashboard.Models;

public class DashboardViewModel
{
    public IEnumerable<StrategicGoal> StrategicGoals { get; set; } = new List<StrategicGoal>();
    
    // Recent Activity Data
    public List<RecentActivity> RecentActivities { get; set; } = new List<RecentActivity>();
    
    // Summary Statistics
    public DashboardSummary Summary { get; set; } = new DashboardSummary();
    
    // Data source information
    public string DataSource { get; set; } = "Database";
    public string Message { get; set; } = "";
    public bool HasError { get; set; } = false;
    public string ErrorMessage { get; set; } = "";
}

public class RecentActivity
{
    public string Type { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime Date { get; set; }
    public string Icon { get; set; } = "";
    public string Color { get; set; } = "";
    public string GoalName { get; set; } = "";
}

public class DashboardSummary
{
    public int TotalStaffSurveys { get; set; }
    public int TotalProfessionalDevelopmentPlans { get; set; }
    public int TotalMediaPlacements { get; set; }
    public int TotalWebsiteTrafficEntries { get; set; }
    public int TotalEvents { get; set; }
    public decimal AverageStaffSatisfaction { get; set; }
    public int TotalActivities { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.Now;
}