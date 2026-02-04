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
    
    // Chart Data
    public ChartData Charts { get; set; } = new ChartData();
    
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

public class ChartData
{
    public GoalProgressData GoalProgress { get; set; } = new GoalProgressData();
    public List<MonthlyTrendData> MonthlyTrends { get; set; } = new List<MonthlyTrendData>();
    public List<MetricTypeData> MetricTypes { get; set; } = new List<MetricTypeData>();
    public List<QuarterlyData> QuarterlyData { get; set; } = new List<QuarterlyData>();
}

public class GoalProgressData
{
    public decimal OrganizationalProgress { get; set; }
    public decimal FinancialProgress { get; set; }
    public decimal IdentityProgress { get; set; }
    public decimal CommunityProgress { get; set; }
}

public class MonthlyTrendData
{
    public string Month { get; set; } = "";
    public decimal OrganizationalValue { get; set; }
    public decimal FinancialValue { get; set; }
    public decimal IdentityValue { get; set; }
    public decimal CommunityValue { get; set; }
}

public class MetricTypeData
{
    public string Type { get; set; } = "";
    public int Count { get; set; }
}

public class QuarterlyData
{
    public string Quarter { get; set; } = "";
    public decimal Value { get; set; }
}