using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneJax.StrategicDashboard.Models;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using System.Collections.Generic;
using System.Linq;
//emily
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index(string status, string time, string goal, string fiscalYear, string quarter)
    {
        try 
        {
            // Create dashboard data from real database entries only
            var dashboardData = BuildDashboardFromRealData();
            
            // Apply filters
            dashboardData = ApplyFilters(dashboardData, status, time, goal, fiscalYear, quarter);

            return View(dashboardData);
        }
        catch (Exception ex)
        {
            // Show error message but with empty data to encourage real data entry
            var errorData = new DashboardViewModel 
            { 
                StrategicGoals = new List<StrategicGoal>(),
                HasError = true,
                ErrorMessage = $"Error accessing database: {ex.Message}. Please ensure data has been entered through the data entry forms.",
                DataSource = "Error - No Data"
            };
            
            return View(errorData);
        }
    }

    private DashboardViewModel ApplyFilters(DashboardViewModel dashboard, string status, string time, string goal, string fiscalYear, string quarter)
    {
        // Filter goals if specific goal is requested
        if (!string.IsNullOrEmpty(goal))
        {
            dashboard.StrategicGoals = dashboard.StrategicGoals.Where(g => g.Name == goal).ToList();
        }

        // Apply time-based filters
        var timeFilter = GetTimeFilter(time, fiscalYear, quarter);

        foreach (var g in dashboard.StrategicGoals)
        {
            if (g.Events != null)
            {
                g.Events = g.Events
                    .Where(e => 
                    {
                        // Status filter
                        var statusMatch = string.IsNullOrEmpty(status) || e.Status == status;
                        
                        // Time filter
                        var timeMatch = timeFilter == null || 
                                       (e.DueDate >= timeFilter.Value.StartDate && e.DueDate <= timeFilter.Value.EndDate);
                        
                        return statusMatch && timeMatch;
                    })
                    .ToList();
            }

            // Apply time filters to metrics if they have date information
            if (g.Metrics != null && timeFilter != null)
            {
                g.Metrics = g.Metrics
                    .Where(m => m.TargetDate >= timeFilter.Value.StartDate && m.TargetDate <= timeFilter.Value.EndDate)
                    .ToList();
            }
        }

        return dashboard;
    }

    private (DateTime StartDate, DateTime EndDate)? GetTimeFilter(string time, string fiscalYear, string quarter)
    {
        var currentDate = DateTime.Now;
        
        // If specific fiscal year and quarter are provided
        if (!string.IsNullOrEmpty(fiscalYear) && !string.IsNullOrEmpty(quarter))
        {
            if (int.TryParse(fiscalYear, out int year) && int.TryParse(quarter.Replace("Q", ""), out int q))
            {
                return GetFiscalQuarterRange(year, q);
            }
        }
        
        // If only fiscal year is provided
        if (!string.IsNullOrEmpty(fiscalYear))
        {
            if (int.TryParse(fiscalYear, out int year))
            {
                return GetFiscalYearRange(year);
            }
        }
        
        // Legacy time filter support
        if (!string.IsNullOrEmpty(time))
        {
            return time switch
            {
                "Current Quarter" => GetCurrentQuarter(currentDate),
                "Q1 2025" => GetFiscalQuarterRange(2025, 1),
                "Q2 2025" => GetFiscalQuarterRange(2025, 2),
                "Q3 2025" => GetFiscalQuarterRange(2025, 3),
                "Q4 2025" => GetFiscalQuarterRange(2025, 4),
                "Q1 2026" => GetFiscalQuarterRange(2026, 1),
                "FY 2025" => GetFiscalYearRange(2025),
                "FY 2026" => GetFiscalYearRange(2026),
                "This Year" => (new DateTime(currentDate.Year, 1, 1), new DateTime(currentDate.Year, 12, 31)),
                _ => null
            };
        }
        
        return null;
    }

    private (DateTime StartDate, DateTime EndDate) GetFiscalYearRange(int fiscalYear)
    {
        // Assuming fiscal year starts July 1 and ends June 30
        var startDate = new DateTime(fiscalYear - 1, 7, 1);
        var endDate = new DateTime(fiscalYear, 6, 30);
        return (startDate, endDate);
    }

    private (DateTime StartDate, DateTime EndDate) GetFiscalQuarterRange(int fiscalYear, int quarter)
    {
        var fyStart = new DateTime(fiscalYear - 1, 7, 1);
        
        return quarter switch
        {
            1 => (fyStart, fyStart.AddMonths(3).AddDays(-1)), // Q1: July-Sep
            2 => (fyStart.AddMonths(3), fyStart.AddMonths(6).AddDays(-1)), // Q2: Oct-Dec  
            3 => (fyStart.AddMonths(6), fyStart.AddMonths(9).AddDays(-1)), // Q3: Jan-Mar
            4 => (fyStart.AddMonths(9), fyStart.AddMonths(12).AddDays(-1)), // Q4: Apr-Jun
            _ => GetFiscalYearRange(fiscalYear)
        };
    }

    private (DateTime StartDate, DateTime EndDate) GetCurrentQuarter(DateTime currentDate)
    {
        // Determine current fiscal quarter based on current date
        var currentFiscalYear = currentDate.Month >= 7 ? currentDate.Year + 1 : currentDate.Year;
        
        var quarter = currentDate.Month switch
        {
            >= 7 and <= 9 => 1,   // July-Sep
            >= 10 and <= 12 => 2, // Oct-Dec
            >= 1 and <= 3 => 3,   // Jan-Mar
            >= 4 and <= 6 => 4,   // Apr-Jun
            _ => 1
        };
        
        return GetFiscalQuarterRange(currentFiscalYear, quarter);
    }

    private DashboardViewModel BuildDashboardFromRealData()
    {
        var dashboard = new DashboardViewModel();
        
        // Build summary statistics from real data
        dashboard.Summary = BuildDashboardSummary();
        
        // Build recent activities from real data
        dashboard.RecentActivities = BuildRecentActivities();

        // Only generate goals if we have real data entries
        var generatedGoals = GenerateGoalsFromRealDataOnly();
        
        // Always ensure we have the four strategic goals as tabs, even if empty
        var allGoals = EnsureFourStrategicGoals(generatedGoals);
        
        // Check if we have any data (database records OR strategy events)
        var hasStrategyEvents = StrategyController.Strategies.Any();
        
        if (allGoals.Any(g => g.Metrics.Any() || g.Events.Any()) || hasStrategyEvents)
        {
            dashboard.StrategicGoals = allGoals;
            dashboard.DataSource = hasStrategyEvents ? "Real Data Entries + Strategy Events" : "Real Data Entries";
            dashboard.Message = hasStrategyEvents ? 
                "Dashboard generated from your actual data entries and strategy events from the Core Strategies tab." :
                "Dashboard generated from your actual data entries including staff surveys, professional development plans, media placements, and website traffic data.";
        }
        else
        {
            // Show empty goal structure to encourage data entry
            dashboard.StrategicGoals = allGoals; // Will be empty goals but with proper structure
            dashboard.DataSource = "No Data Available";
            dashboard.Message = "No data entries found. Please submit data through the following forms: Staff Surveys, Professional Development, Media Placements, and Website Traffic to see your dashboard populate with real metrics.";
        }

        return dashboard;
    }

    private List<StrategicGoal> EnsureFourStrategicGoals(List<StrategicGoal> existingGoals)
    {
        var allGoals = new List<StrategicGoal>();
        
        // Define the four strategic goals structure (matching StrategyController mapping)
        var goalTemplates = new List<(int Id, string Name, string Description, string Color)>
        {
            (1, "Organizational Building", "Strengthening organizational structure and capacity", "var(--onejax-navy)"),
            (2, "Financial Sustainability", "Ensuring sustainable financial health and growth", "var(--onejax-green)"),
            (3, "Identity/Value Proposition", "Establishing and communicating OneJax's unique identity and value", "var(--onejax-orange)"),
            (4, "Community Engagement", "Building partnerships and community connections", "var(--onejax-blue)")
        };

        foreach (var template in goalTemplates)
        {
            // Find existing goal with data or create empty one
            var existingGoal = existingGoals.FirstOrDefault(g => g.Id == template.Id || g.Name == template.Name);
            
            if (existingGoal != null)
            {
                // Use the goal with real data
                existingGoal.Id = template.Id; // Ensure consistent ID
                existingGoal.Name = template.Name; // Ensure consistent name
                existingGoal.Color = template.Color; // Ensure consistent color
                
                // Add events from Strategy controller's static list
                existingGoal.Events.AddRange(GetEventsFromStrategyController(template.Id));
                
                allGoals.Add(existingGoal);
            }
            else
            {
                // Create empty goal structure for tab consistency
                var newGoal = new StrategicGoal
                {
                    Id = template.Id,
                    Name = template.Name,
                    Description = template.Description,
                    Color = template.Color,
                    Events = new List<Event>(),
                    Metrics = new List<GoalMetric>()
                };
                
                // Add events from Strategy controller's static list
                newGoal.Events.AddRange(GetEventsFromStrategyController(template.Id));
                
                allGoals.Add(newGoal);
            }
        }

        return allGoals;
    }

    private List<Event> GetEventsFromStrategyController(int strategicGoalId)
    {
        var events = new List<Event>();
        
        // Get events from database first (persistent)
        var dbEvents = _context.Events.Where(e => e.StrategicGoalId == strategicGoalId).ToList();
        events.AddRange(dbEvents);
        
        // Also get events from StrategyController's static list (for backward compatibility)
        var strategies = StrategyController.Strategies.Where(s => s.StrategicGoalId == strategicGoalId).ToList();
        
        foreach (var strategy in strategies)
        {
            // Only add if not already in database
            if (!dbEvents.Any(e => e.Title == strategy.Name && e.Description == strategy.Description))
            {
                events.Add(new Event
                {
                    Id = strategy.Id,
                    Title = strategy.Name,
                    Description = strategy.Description,
                    Type = strategy.EventType ?? "Community", // Use event type from strategy
                    Status = "Planned", // Default status
                    StrategicGoalId = strategicGoalId,
                    DueDate = DateTime.TryParse(strategy.Date, out var date) ? date : DateTime.Now.AddDays(30),
                    Notes = $"Added through Core Strategies tab. {(!string.IsNullOrEmpty(strategy.Time) ? $"Time: {strategy.Time}" : "")}",
                    Attendees = 0
                });
            }
        }
        
        return events;
    }

    private DashboardSummary BuildDashboardSummary()
    {
        var summary = new DashboardSummary();
        
        try
        {
            // Staff Surveys
            var staffSurveys = _context.StaffSurveys_22D.ToList();
            summary.TotalStaffSurveys = staffSurveys.Count;
            summary.AverageStaffSatisfaction = staffSurveys.Any() ? 
                (decimal)staffSurveys.Average(s => s.SatisfactionRate) : 0;

            // Professional Development
            summary.TotalProfessionalDevelopmentPlans = _context.ProfessionalDevelopments.Count();

            // Media Placements
            summary.TotalMediaPlacements = _context.MediaPlacements_3D.Count();

            // Website Traffic
            summary.TotalWebsiteTrafficEntries = _context.WebsiteTraffic.Count();

            // Events (from database or generated)
            try
            {
                summary.TotalEvents = _context.Events.Count();
            }
            catch
            {
                // Events table might not exist yet
                summary.TotalEvents = 0;
            }

            // Calculate total activities
            summary.TotalActivities = summary.TotalStaffSurveys + 
                                    summary.TotalProfessionalDevelopmentPlans + 
                                    summary.TotalMediaPlacements + 
                                    summary.TotalWebsiteTrafficEntries +
                                    summary.TotalEvents;

            summary.LastUpdated = DateTime.Now;
        }
        catch (Exception)
        {
            // Return empty summary if database access fails
        }

        return summary;
    }

    private List<RecentActivity> BuildRecentActivities()
    {
        var activities = new List<RecentActivity>();

        try
        {
            // Add recent staff surveys
            var recentSurveys = _context.StaffSurveys_22D
                .OrderByDescending(s => s.CreatedDate)
                .Take(5)
                .ToList();

            foreach (var survey in recentSurveys)
            {
                activities.Add(new RecentActivity
                {
                    Type = "Staff Survey",
                    Title = $"Staff Survey Completed",
                    Description = $"{survey.Name} completed satisfaction survey (Rate: {survey.SatisfactionRate}%)",
                    Date = survey.CreatedDate,
                    Icon = "fas fa-user-check",
                    Color = "var(--onejax-blue)",
                    GoalName = "Organizational Building"
                });
            }

            // Add recent professional development entries
            var recentProfDev = _context.ProfessionalDevelopments
                .OrderByDescending(p => p.CreatedDate)
                .Take(5)
                .ToList();

            foreach (var profDev in recentProfDev)
            {
                activities.Add(new RecentActivity
                {
                    Type = "Professional Development",
                    Title = "Development Plan Submitted",
                    Description = $"{profDev.Name} planned {profDev.ProfessionalDevelopmentYear26 + profDev.ProfessionalDevelopmentYear27} activities",
                    Date = profDev.CreatedDate,
                    Icon = "fas fa-graduation-cap",
                    Color = "var(--onejax-green)",
                    GoalName = "Organizational Building"
                });
            }

            // Add recent media placements
            var recentMedia = _context.MediaPlacements_3D
                .OrderByDescending(m => m.CreatedDate)
                .Take(3)
                .ToList();

            foreach (var media in recentMedia)
            {
                var totalPlacements = (media.January ?? 0) + (media.February ?? 0) + 
                                    (media.March ?? 0) + (media.April ?? 0) + 
                                    (media.May ?? 0) + (media.June ?? 0) + 
                                    (media.July ?? 0) + (media.August ?? 0) + 
                                    (media.September ?? 0) + (media.October ?? 0) + 
                                    (media.November ?? 0) + (media.December ?? 0);
                
                activities.Add(new RecentActivity
                {
                    Type = "Media Placement",
                    Title = "Media Placements Updated",
                    Description = $"Total of {totalPlacements} media placements recorded",
                    Date = media.CreatedDate,
                    Icon = "fas fa-newspaper",
                    Color = "var(--onejax-orange)",
                    GoalName = "Identity/Value Proposition"
                });
            }

            // Add recent website traffic entries
            var recentTraffic = _context.WebsiteTraffic
                .OrderByDescending(w => w.CreatedDate)
                .Take(3)
                .ToList();

            foreach (var traffic in recentTraffic)
            {
                activities.Add(new RecentActivity
                {
                    Type = "Website Traffic",
                    Title = "Website Traffic Recorded",
                    Description = $"Total clicks: {traffic.TotalClicks}",
                    Date = traffic.CreatedDate,
                    Icon = "fas fa-mouse-pointer",
                    Color = "var(--onejax-navy)",
                    GoalName = "Community Engagement"
                });
            }

            // Sort all activities by date and take most recent
            activities = activities.OrderByDescending(a => a.Date).Take(10).ToList();
        }
        catch (Exception)
        {
            // Return empty list if database access fails
        }

        return activities;
    }

    private List<StrategicGoal> GenerateGoalsFromRealDataOnly()
    {
        var goals = new List<StrategicGoal>();

        // Get all data from database
        var staffSurveys = _context.StaffSurveys_22D.ToList();
        var profDev = _context.ProfessionalDevelopments.ToList();
        var mediaPlacements = _context.MediaPlacements_3D.ToList();
        var websiteTraffic = _context.WebsiteTraffic.ToList();

        // Only create goals if we have real data
        if (staffSurveys.Any() || profDev.Any() || mediaPlacements.Any() || websiteTraffic.Any())
        {
            // Goal 1: Organizational Building (only if we have staff/prof dev data)
            if (staffSurveys.Any() || profDev.Any())
            {
                var orgGoal = CreateOrganizationalBuildingGoalFromRealData(staffSurveys, profDev);
                goals.Add(orgGoal);
            }

            // Goal 2: Financial Sustainability (placeholder for now, no specific data yet)
            // We'll create an empty goal for this when we have financial data

            // Goal 3: Identity/Value Proposition (only if we have media placement data)
            if (mediaPlacements.Any())
            {
                var identityGoal = CreateIdentityGoalFromRealData(mediaPlacements);
                goals.Add(identityGoal);
            }

            // Goal 4: Community Engagement (only if we have website traffic data)
            if (websiteTraffic.Any())
            {
                var communityGoal = CreateCommunityEngagementGoalFromRealData(websiteTraffic);
                goals.Add(communityGoal);
            }

            // Goal 4: Financial Stability (only add if we eventually have financial data)
            // For now, we'll skip this since we don't have financial form data yet
        }

        return goals;
    }

    private StrategicGoal CreateOrganizationalBuildingGoalFromRealData(List<StaffSurvey_22D> staffSurveys, List<ProfessionalDevelopment> profDev)
    {
        var goal = new StrategicGoal
        {
            Id = 1,
            Name = "Organizational Building",
            Description = "Staff development and organizational capacity",
            Color = "var(--onejax-navy)",
            Events = new List<Event>(),
            Metrics = new List<GoalMetric>()
        };

        // Generate metrics from staff surveys
        if (staffSurveys.Any())
        {
            var avgSatisfaction = staffSurveys.Average(s => s.SatisfactionRate);
            var totalStaff = staffSurveys.Count;
            
            goal.Metrics.Add(new GoalMetric
            {
                Id = 1,
                Name = "Staff Satisfaction Rate",
                Description = $"Based on {totalStaff} staff survey responses",
                StrategicGoalId = 1,
                Target = "85",
                CurrentValue = (decimal)Math.Round(avgSatisfaction, 1),
                Unit = "%",
                Status = avgSatisfaction >= 85 ? "On Track" : "Needs Attention",
                TargetDate = DateTime.Now.AddMonths(6)
            });

            var totalProfDevFromSurveys = staffSurveys.Sum(s => s.ProfessionalDevelopmentCount);
            
            goal.Metrics.Add(new GoalMetric
            {
                Id = 2,
                Name = "Professional Development Activities (Staff Reported)",
                Description = $"Activities reported by staff members",
                StrategicGoalId = 1,
                Target = "50",
                CurrentValue = totalProfDevFromSurveys,
                Unit = "activities",
                Status = totalProfDevFromSurveys >= 50 ? "On Track" : "In Progress",
                TargetDate = DateTime.Now.AddMonths(6)
            });

            // Metrics only - no automatic event creation for data entry
        }

        // Generate metrics from professional development data
        if (profDev.Any())
        {
            var totalDev26 = profDev.Sum(p => p.ProfessionalDevelopmentYear26);
            var totalDev27 = profDev.Sum(p => p.ProfessionalDevelopmentYear27);
            
            goal.Metrics.Add(new GoalMetric
            {
                Id = 3,
                Name = "Professional Development Planning",
                Description = $"Planned activities for 2026-2027",
                StrategicGoalId = 1,
                Target = "100",
                CurrentValue = totalDev26 + totalDev27,
                Unit = "activities",
                Status = (totalDev26 + totalDev27) >= 100 ? "On Track" : "In Progress",
                TargetDate = DateTime.Now.AddMonths(12),
                Q1Value = totalDev26,
                Q2Value = totalDev27
            });

            // Metrics only - no automatic event creation for data entry
        }

        return goal;
    }

    private StrategicGoal CreateIdentityGoalFromRealData(List<MediaPlacements_3D> mediaPlacements)
    {
        var goal = new StrategicGoal
        {
            Id = 3, // Changed from 2 to 3 to match StrategyController mapping
            Name = "Identity/Value Proposition",
            Description = "Establishing and communicating OneJax's unique identity and value",
            Color = "var(--onejax-orange)",
            Events = new List<Event>(),
            Metrics = new List<GoalMetric>()
        };

        if (mediaPlacements.Any())
        {
            var totalPlacements = 0;

            foreach (var placement in mediaPlacements)
            {
                var placementTotal = (placement.January ?? 0) + (placement.February ?? 0) + 
                                   (placement.March ?? 0) + (placement.April ?? 0) + 
                                   (placement.May ?? 0) + (placement.June ?? 0) + 
                                   (placement.July ?? 0) + (placement.August ?? 0) + 
                                   (placement.September ?? 0) + (placement.October ?? 0) + 
                                   (placement.November ?? 0) + (placement.December ?? 0);
                totalPlacements += placementTotal;
            }

            goal.Metrics.Add(new GoalMetric
            {
                Id = 4,
                Name = "Media Placements",
                Description = $"Total media placements across all channels",
                StrategicGoalId = 3, // Changed from 2 to 3
                Target = "200",
                CurrentValue = totalPlacements,
                Unit = "placements",
                Status = totalPlacements >= 200 ? "On Track" : "In Progress",
                TargetDate = DateTime.Now.AddMonths(6)
            });

            goal.Metrics.Add(new GoalMetric
            {
                Id = 5,
                Name = "Media Coverage Frequency",
                Description = "Average monthly media presence",
                StrategicGoalId = 3, // Changed from 2 to 3
                Target = "15",
                CurrentValue = (decimal)(totalPlacements / 12.0),
                Unit = "per month",
                Status = (totalPlacements / 12.0) >= 15 ? "On Track" : "Needs Attention",
                TargetDate = DateTime.Now.AddMonths(6)
            });

            // Metrics only - no automatic event creation for data entry
        }

        return goal;
    }

    private StrategicGoal CreateCommunityEngagementGoalFromRealData(List<WebsiteTraffic_4D> websiteTraffic)
    {
        var goal = new StrategicGoal
        {
            Id = 4, // Changed from 3 to 4 to match StrategyController mapping
            Name = "Community Engagement",
            Description = "Building partnerships and community connections",
            Color = "var(--onejax-blue)",
            Events = new List<Event>(),
            Metrics = new List<GoalMetric>()
        };

        if (websiteTraffic.Any())
        {
            var totalClicks = websiteTraffic.Sum(w => w.TotalClicks);
            var avgQuarterlyClicks = websiteTraffic.Average(w => w.TotalClicks);

            goal.Metrics.Add(new GoalMetric
            {
                Id = 6,
                Name = "Website Traffic",
                Description = "Total website clicks across all quarters",
                StrategicGoalId = 4, // Changed from 3 to 4
                Target = "10000",
                CurrentValue = totalClicks,
                Unit = "clicks",
                Status = totalClicks >= 10000 ? "On Track" : "In Progress",
                TargetDate = DateTime.Now.AddMonths(6)
            });

            goal.Metrics.Add(new GoalMetric
            {
                Id = 7,
                Name = "Digital Engagement Rate",
                Description = "Average quarterly website engagement",
                StrategicGoalId = 4, // Changed from 3 to 4
                Target = "2500",
                CurrentValue = (decimal)avgQuarterlyClicks,
                Unit = "clicks/quarter",
                Status = avgQuarterlyClicks >= 2500 ? "On Track" : "Needs Improvement",
                TargetDate = DateTime.Now.AddMonths(3)
            });

            // Metrics only - no automatic event creation for data entry
        }

        return goal;
    }
}
