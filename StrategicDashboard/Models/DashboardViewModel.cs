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

    // Identity map data
    public Dictionary<string, int> ZipCoverage { get; set; } = new Dictionary<string, int>();

    // Identity/Value Proposition visual cards (derived from Data Entry tables)
    public IdentityDashboardData Identity { get; set; } = new IdentityDashboardData();
    
    // Data source information
    public string DataSource { get; set; } = "Database";
    public string Message { get; set; } = "";
    public bool HasError { get; set; } = false;
    public string ErrorMessage { get; set; } = "";
}

public class IdentityDashboardData
{
    public int MediaPlacementsTotal { get; set; }
    public int MediaPlacementsTarget { get; set; } = 50;
    public DateTime? MediaPlacementsLastUpdated { get; set; }
    public int[] MediaPlacementsByMonth { get; set; } = new int[12];

    public int WebsiteClicksTotal { get; set; }
    public int WebsiteClicksTarget { get; set; } = 4000;
    public DateTime? WebsiteTrafficLastUpdated { get; set; }
    public int[] WebsiteClicksByQuarter { get; set; } = new int[4];

    public decimal TrustPercent { get; set; }
    public decimal TrustTargetPercent { get; set; } = 70m;
    public int? TrustRespondents { get; set; }
    public int? TrustYear { get; set; }
    public DateTime? TrustLastUpdated { get; set; }
    public List<int> TrustHistoryYears { get; set; } = new List<int>();
    public List<decimal> TrustHistoryPercents { get; set; } = new List<decimal>();

    public decimal MilestonePercent { get; set; }
    public decimal MilestoneTargetPercent { get; set; } = 75m;
    public bool MilestoneReviewActive { get; set; }
    public DateTime? MilestoneLastUpdated { get; set; }

    public int? SocialYear { get; set; }
    public decimal SocialAvgEngagementRate { get; set; }
    public decimal? SocialQ1 { get; set; }
    public decimal? SocialQ2 { get; set; }
    public decimal? SocialQ3 { get; set; }
    public decimal? SocialQ4 { get; set; }
    public bool SocialGoalMet { get; set; }
    public DateTime? SocialLastUpdated { get; set; }

    public int? FrameworkYear { get; set; }
    public string FrameworkQuarter { get; set; } = "";
    public string FrameworkStatus { get; set; } = "";
    public bool FrameworkGoalMet { get; set; }
    public DateTime? FrameworkLastUpdated { get; set; }

    public int ZipCodesServed { get; set; }
    public int ZipCodeGoal { get; set; } = 25;
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
