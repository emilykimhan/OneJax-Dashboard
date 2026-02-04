using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneJax.StrategicDashboard.Models;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using OneJaxDashboard.Services;
using System.Collections.Generic;
using System.Linq;
//emily
public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly MetricsService _metricsService;

    public HomeController(ApplicationDbContext context, MetricsService metricsService)
    {
        _context = context;
        _metricsService = metricsService;
    }

    public async Task<IActionResult> Index(string status, string time, string goal, string fiscalYear, string quarter)
    {
        try 
        {
            // Create enhanced dashboard data with comprehensive metrics
            var dashboardData = await BuildEnhancedDashboardAsync(fiscalYear ?? "2025-2026");
            
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
        
        // Only generate goals if we have real data entries
        var generatedGoals = GenerateGoalsFromRealDataOnly();
        
        // Always ensure we have the four strategic goals as tabs, even if empty
        var allGoals = EnsureFourStrategicGoals(generatedGoals);

        // Build summary statistics from real data
        dashboard.Summary = BuildDashboardSummary();
        
        // Build recent activities from real data
        dashboard.RecentActivities = BuildRecentActivities();

        // Build chart data from real metrics
        dashboard.Charts = BuildChartData(allGoals);
        
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
                    Color = "var(--onejax-orange)",
                    GoalName = "Identity/Value Proposition"
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

            // Goal 3: Identity/Value Proposition (media placements and website traffic)
            if (mediaPlacements.Any() || websiteTraffic.Any())
            {
                var identityGoal = CreateIdentityGoalFromRealData(mediaPlacements, websiteTraffic);
                goals.Add(identityGoal);
            }

            // Goal 4: Community Engagement (will be populated by comprehensive metrics)
            // This goal doesn't depend on specific database tables yet
            // It will be enhanced with metrics from MetricsService
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
                Target = "",
                CurrentValue = (decimal)Math.Round(avgSatisfaction, 1),
                Unit = "%",
                Status = "In Progress",
                TargetDate = DateTime.Now.AddMonths(12)
            });

            var totalProfDevFromSurveys = staffSurveys.Sum(s => s.ProfessionalDevelopmentCount);
            
            goal.Metrics.Add(new GoalMetric
            {
                Id = 2,
                Name = "Professional Development Activities (Staff Reported)",
                Description = $"Activities reported by staff members",
                StrategicGoalId = 1,
                Target = "",
                CurrentValue = totalProfDevFromSurveys,
                Unit = "activities",
                Status = "In Progress",
                TargetDate = DateTime.Now.AddMonths(12)
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
                Target = "",
                CurrentValue = totalDev26 + totalDev27,
                Unit = "activities",
                Status = "In Progress",
                TargetDate = DateTime.Now.AddMonths(12),
                Q1Value = totalDev26,
                Q2Value = totalDev27
            });

            // Metrics only - no automatic event creation for data entry
        }

        return goal;
    }

    private StrategicGoal CreateIdentityGoalFromRealData(List<MediaPlacements_3D> mediaPlacements, List<WebsiteTraffic_4D>? websiteTraffic = null)
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
                StrategicGoalId = 3,
                Target = "",
                CurrentValue = totalPlacements,
                Unit = "placements",
                Status = "In Progress",
                TargetDate = DateTime.Now.AddMonths(12)
            });

            goal.Metrics.Add(new GoalMetric
            {
                Id = 5,
                Name = "Media Coverage Frequency",
                Description = "Average monthly media presence",
                StrategicGoalId = 3,
                Target = "",
                CurrentValue = (decimal)(totalPlacements / 12.0),
                Unit = "per month",
                Status = "In Progress",
                TargetDate = DateTime.Now.AddMonths(12)
            });
        }

        // Add website traffic metrics to Identity goal (where they belong)
        // Note: MetricsService will provide "Website Traffic Q1" metric with proper targets
        // We don't need to create duplicate metrics here since comprehensive metrics
        // from MetricsService will be enhanced with real data in UpdateMetricsWithRealDataAsync

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

        // Community Engagement should not include website traffic metrics
        // Website traffic belongs to Identity/Value Proposition goal
        // This goal will be populated by MetricsService with appropriate community metrics
        // like Joint Initiative Satisfaction, Cross-Sector Collaborations, etc.

        return goal;
    }

    private async Task<DashboardViewModel> BuildEnhancedDashboardAsync(string fiscalYear = "2025-2026")
    {
        // Start with the existing real data dashboard
        var dashboard = BuildDashboardFromRealData();
        
        // Enhance each strategic goal with comprehensive metrics
        await EnhanceGoalsWithComprehensiveMetricsAsync(dashboard.StrategicGoals.ToList(), fiscalYear);
        
        // Update dashboard message to reflect enhanced metrics
        if (dashboard.StrategicGoals.Any(g => g.Metrics.Any(m => m.DataSource != null)))
        {
            dashboard.DataSource += " + Comprehensive Metrics";
            dashboard.Message = "Dashboard enhanced with your comprehensive strategic metrics including targets, progress tracking, and multi-year planning.";
        }
        
        return dashboard;
    }
    
    private async Task EnhanceGoalsWithComprehensiveMetricsAsync(List<StrategicGoal> goals, string fiscalYear)
    {
        // Initialize metrics if they don't exist
        await _metricsService.SeedDashboardMetricsAsync();
        
        foreach (var goal in goals)
        {
            // Get comprehensive metrics for this goal
            List<GoalMetric> comprehensiveMetrics;
            
            if (goal.Name.Contains("Identity"))
                comprehensiveMetrics = await _metricsService.GetPublicMetricsAsync("Identity", fiscalYear);
            else if (goal.Name.Contains("Community"))
                comprehensiveMetrics = await _metricsService.GetPublicMetricsAsync("Community", fiscalYear);
            else if (goal.Name.Contains("Financial"))
                comprehensiveMetrics = await _metricsService.GetPublicMetricsAsync("Financial", fiscalYear);
            else if (goal.Name.Contains("Organizational"))
                comprehensiveMetrics = await _metricsService.GetPublicMetricsAsync("Organizational", fiscalYear);
            else
                continue;
            
            // Add comprehensive metrics to existing ones (avoid duplicates)
            foreach (var metric in comprehensiveMetrics)
            {
                if (!goal.Metrics.Any(m => m.Name == metric.Name))
                {
                    goal.Metrics.Add(metric);
                }
            }
            
            // Update metrics with real data from database
            await UpdateMetricsWithRealDataAsync(goal);
        }
    }
    
    private async Task UpdateMetricsWithRealDataAsync(StrategicGoal goal)
    {
        try
        {
            // Update Identity/Value Proposition metrics with media placement data
        if (goal.Name.Contains("Identity"))
        {
            // Get total media placements from database
            var mediaPlacements = await _context.MediaPlacements_3D.ToListAsync();
            var totalPlacements = 0;
            
            foreach (var placement in mediaPlacements)
            {
                totalPlacements += (placement.January ?? 0) + (placement.February ?? 0) + 
                                 (placement.March ?? 0) + (placement.April ?? 0) + 
                                 (placement.May ?? 0) + (placement.June ?? 0) + 
                                 (placement.July ?? 0) + (placement.August ?? 0) + 
                                 (placement.September ?? 0) + (placement.October ?? 0) + 
                                 (placement.November ?? 0) + (placement.December ?? 0);
            }
            
            // Update Earned Media Placements metric
            var mediaPlacementMetric = goal.Metrics.FirstOrDefault(m => m.Name == "Earned Media Placements");
            if (mediaPlacementMetric != null)
            {
                // Update the metric with real data
                mediaPlacementMetric.CurrentValue = totalPlacements;
            }
            
            // Add or update Media Coverage Frequency metric
            var frequencyMetric = goal.Metrics.FirstOrDefault(m => m.Name == "Media Coverage Frequency");
            if (frequencyMetric == null && totalPlacements > 0)
            {
                // Create the frequency metric if it doesn't exist
                goal.Metrics.Add(new GoalMetric
                {
                    Id = goal.Metrics.Count + 100, // Unique ID
                    Name = "Media Coverage Frequency",
                    Description = "Average monthly media presence",
                    StrategicGoalId = goal.Id,
                    Target = "1.0",
                    CurrentValue = Math.Round((decimal)(totalPlacements / 12.0), 1),
                    Unit = "per month",
                    DataSource = "Form",
                    MetricType = "Count",
                    IsPublic = true,
                    FiscalYear = "2025-2026",
                    Status = "Active",
                    TargetDate = DateTime.Now.AddMonths(12)
                });
            }
            else if (frequencyMetric != null)
            {
                // Update existing frequency metric
                frequencyMetric.CurrentValue = Math.Round((decimal)(totalPlacements / 12.0), 1);
            }
            
            // Update website traffic metrics if they exist
            var websiteTrafficAnnualMetric = goal.Metrics.FirstOrDefault(m => m.Name == "Website Traffic (Annual)");
            if (websiteTrafficAnnualMetric != null)
            {
                var websiteTraffic = await _context.WebsiteTraffic.ToListAsync();
                if (websiteTraffic.Any())
                {
                    // Calculate total annual traffic
                    var q1Total = websiteTraffic.Sum(w => w.Q1_JulySeptember ?? 0);
                    var q2Total = websiteTraffic.Sum(w => w.Q2_OctoberDecember ?? 0);
                    var q3Total = websiteTraffic.Sum(w => w.Q3_JanuaryMarch ?? 0);
                    var q4Total = websiteTraffic.Sum(w => w.Q4_AprilJune ?? 0);
                    
                    // Update with total annual traffic
                    websiteTrafficAnnualMetric.CurrentValue = q1Total + q2Total + q3Total + q4Total;
                    
                    // Store quarterly values for detailed display
                    websiteTrafficAnnualMetric.Q1Value = q1Total;
                    websiteTrafficAnnualMetric.Q2Value = q2Total;
                    websiteTrafficAnnualMetric.Q3Value = q3Total;
                    websiteTrafficAnnualMetric.Q4Value = q4Total;
                    
                    // Update the description to show quarterly breakdown dynamically
                    var quarterlyBreakdown = new List<string>();
                    if (q1Total > 0) quarterlyBreakdown.Add($"Q1: {q1Total:N0}");
                    if (q2Total > 0) quarterlyBreakdown.Add($"Q2: {q2Total:N0}");
                    if (q3Total > 0) quarterlyBreakdown.Add($"Q3: {q3Total:N0}");
                    if (q4Total > 0) quarterlyBreakdown.Add($"Q4: {q4Total:N0}");
                    
                    if (quarterlyBreakdown.Any())
                    {
                        websiteTrafficAnnualMetric.Description = $"Total: {websiteTrafficAnnualMetric.CurrentValue:N0} clicks ({string.Join(", ", quarterlyBreakdown)})";
                    }
                    else
                    {
                        websiteTrafficAnnualMetric.Description = "No data entered yet";
                    }
                }
                else
                {
                    // IMPORTANT: Clear all cached values when no data exists
                    websiteTrafficAnnualMetric.CurrentValue = 0;
                    websiteTrafficAnnualMetric.Q1Value = 0;
                    websiteTrafficAnnualMetric.Q2Value = 0;
                    websiteTrafficAnnualMetric.Q3Value = 0;
                    websiteTrafficAnnualMetric.Q4Value = 0;
                    websiteTrafficAnnualMetric.Description = "No website traffic data entered yet";
                    websiteTrafficAnnualMetric.Status = "Planning";
                }
            }
            
            // You can add other Identity metrics here (website traffic Q2-Q4, etc.)
        }
        
        // Update Organizational Building metrics with staff survey and prof dev data
        if (goal.Name.Contains("Organizational"))
        {
            // Add updates for staff survey metrics here if needed
            var staffSurveys = await _context.StaffSurveys_22D.ToListAsync();
            var profDevs = await _context.ProfessionalDevelopments.ToListAsync();
            
            // Update any staff-related metrics with real counts
            foreach (var metric in goal.Metrics.Where(m => m.DataSource == "Form"))
            {
                if (metric.Name.Contains("Staff") && staffSurveys.Any())
                {
                    metric.CurrentValue = staffSurveys.Count;
                }
                if (metric.Name.Contains("Development") && profDevs.Any())
                {
                    metric.CurrentValue = profDevs.Count;
                }
            }
        }
        
        // Update Community Engagement metrics - no automatic updates from website traffic
        if (goal.Name.Contains("Community"))
        {
            // Community metrics are updated through their respective forms only
            // Website traffic should not appear in Community goals
        }

        // Save all metric updates to the database
        await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log the error but don't break the dashboard
            // In a real application, you'd use proper logging here
            Console.WriteLine($"Error updating metrics: {ex.Message}");
        }
    }

    private ChartData BuildChartData(List<StrategicGoal> goals)
    {
        var chartData = new ChartData();

        // Goal Progress Data - Calculate actual progress from metrics
        var orgGoal = goals.FirstOrDefault(g => g.Name.Contains("Organizational"));
        var finGoal = goals.FirstOrDefault(g => g.Name.Contains("Financial"));
        var identityGoal = goals.FirstOrDefault(g => g.Name.Contains("Identity"));
        var communityGoal = goals.FirstOrDefault(g => g.Name.Contains("Community"));

        chartData.GoalProgress = new GoalProgressData
        {
            OrganizationalProgress = CalculateGoalProgress(orgGoal),
            FinancialProgress = CalculateGoalProgress(finGoal),
            IdentityProgress = CalculateGoalProgress(identityGoal),
            CommunityProgress = CalculateGoalProgress(communityGoal)
        };

        // Monthly Trends - Based on actual data creation dates
        chartData.MonthlyTrends = BuildMonthlyTrends(goals);

        // Metric Types Distribution
        chartData.MetricTypes = BuildMetricTypesData(goals);

        // Quarterly Data from website traffic
        chartData.QuarterlyData = BuildQuarterlyData();

        return chartData;
    }

    private decimal CalculateGoalProgress(StrategicGoal? goal)
    {
        if (goal?.Metrics == null || !goal.Metrics.Any())
            return 0;

        var metricsWithTargets = goal.Metrics.Where(m => 
            !string.IsNullOrEmpty(m.Target) && 
            decimal.TryParse(m.Target, out var target) && 
            target > 0).ToList();

        if (!metricsWithTargets.Any())
            return goal.Metrics.Any() ? 25 : 0; // Default progress if we have metrics but no targets

        var progressValues = metricsWithTargets.Select(m =>
        {
            if (decimal.TryParse(m.Target, out var target) && target > 0)
            {
                var progress = (m.CurrentValue / target) * 100;
                return Math.Min(progress, 100); // Cap at 100%
            }
            return 0m;
        });

        return Math.Round(progressValues.Average(), 1);
    }

    private List<MonthlyTrendData> BuildMonthlyTrends(List<StrategicGoal> goals)
    {
        var trends = new List<MonthlyTrendData>();
        var months = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        for (int i = 0; i < 12; i++)
        {
            var trend = new MonthlyTrendData
            {
                Month = months[i],
                OrganizationalValue = GetMonthlyValue("Organizational", i + 1, goals),
                FinancialValue = GetMonthlyValue("Financial", i + 1, goals),
                IdentityValue = GetMonthlyValue("Identity", i + 1, goals),
                CommunityValue = GetMonthlyValue("Community", i + 1, goals)
            };
            trends.Add(trend);
        }

        return trends;
    }

    private decimal GetMonthlyValue(string goalType, int month, List<StrategicGoal> goals)
    {
        var goal = goals.FirstOrDefault(g => g.Name.Contains(goalType));
        if (goal?.Metrics == null || !goal.Metrics.Any())
            return 0;

        // For demonstration, we'll simulate growth over time based on current metrics
        var baseValue = goal.Metrics.Sum(m => m.CurrentValue) / 12; // Average monthly value
        var growth = month * 0.1m; // 10% growth per month
        return Math.Round(Math.Max(0, baseValue + (baseValue * growth)), 1);
    }

    private List<MetricTypeData> BuildMetricTypesData(List<StrategicGoal> goals)
    {
        var metricTypes = new Dictionary<string, int>();

        foreach (var goal in goals.Where(g => g.Metrics != null))
        {
            foreach (var metric in goal.Metrics)
            {
                // Categorize metrics by type
                var category = CategorizeMetric(metric);
                metricTypes[category] = metricTypes.GetValueOrDefault(category, 0) + 1;
            }
        }

        return metricTypes.Select(kv => new MetricTypeData
        {
            Type = kv.Key,
            Count = kv.Value
        }).ToList();
    }

    private string CategorizeMetric(GoalMetric metric)
    {
        if (metric.Unit.Contains("%")) return "Percentage";
        if (metric.Unit.Contains("activities") || metric.Unit.Contains("events")) return "Activities";
        if (metric.Unit.Contains("placements") || metric.Unit.Contains("media")) return "Media";
        if (metric.Unit.Contains("clicks") || metric.Unit.Contains("traffic")) return "Digital";
        if (metric.Unit.Contains("$") || metric.Unit.Contains("revenue")) return "Financial";
        return "Other";
    }

    private List<QuarterlyData> BuildQuarterlyData()
    {
        var quarterlyData = new List<QuarterlyData>();
        
        try
        {
            var websiteTraffic = _context.WebsiteTraffic.ToList();
            if (websiteTraffic.Any())
            {
                quarterlyData.Add(new QuarterlyData { Quarter = "Q1", Value = websiteTraffic.Sum(w => w.Q1_JulySeptember ?? 0) });
                quarterlyData.Add(new QuarterlyData { Quarter = "Q2", Value = websiteTraffic.Sum(w => w.Q2_OctoberDecember ?? 0) });
                quarterlyData.Add(new QuarterlyData { Quarter = "Q3", Value = websiteTraffic.Sum(w => w.Q3_JanuaryMarch ?? 0) });
                quarterlyData.Add(new QuarterlyData { Quarter = "Q4", Value = websiteTraffic.Sum(w => w.Q4_AprilJune ?? 0) });
            }
            else
            {
                // Show empty quarters if no data
                quarterlyData.AddRange(new[]
                {
                    new QuarterlyData { Quarter = "Q1", Value = 0 },
                    new QuarterlyData { Quarter = "Q2", Value = 0 },
                    new QuarterlyData { Quarter = "Q3", Value = 0 },
                    new QuarterlyData { Quarter = "Q4", Value = 0 }
                });
            }
        }
        catch
        {
            // Return empty data if there's an issue accessing the database
            quarterlyData.AddRange(new[]
            {
                new QuarterlyData { Quarter = "Q1", Value = 0 },
                new QuarterlyData { Quarter = "Q2", Value = 0 },
                new QuarterlyData { Quarter = "Q3", Value = 0 },
                new QuarterlyData { Quarter = "Q4", Value = 0 }
            });
        }

        return quarterlyData;
    }
}
