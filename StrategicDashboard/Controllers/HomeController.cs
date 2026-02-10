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

            // Cross Sector Collaborations
            summary.TotalCrossSectorCollabs = _context.CrossSectorCollabs.Count();
            var activeCrossSectorCollabs = _context.CrossSectorCollabs
                .Where(c => c.Status == "Active")
                .ToList();
            summary.ActiveCrossSectorCollabs = activeCrossSectorCollabs.Count;
            summary.AverageCrossSectorSatisfaction = activeCrossSectorCollabs.Any() ? 
                activeCrossSectorCollabs.Average(c => c.partner_satisfaction_ratings) : 0;

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
                                    summary.TotalCrossSectorCollabs +
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

            // Add recent cross-sector collaborations
            var recentCollabs = _context.CrossSectorCollabs
                .OrderByDescending(c => c.CreatedDate)
                .Take(3)
                .ToList();

            foreach (var collab in recentCollabs)
            {
                activities.Add(new RecentActivity
                {
                    Type = "Cross-Sector Collaboration",
                    Title = collab.Name,
                    Description = $"Status: {collab.Status} | Satisfaction: {collab.partner_satisfaction_ratings}% | Year: {collab.Year}",
                    Date = collab.CreatedDate,
                    Icon = "fas fa-handshake",
                    Color = "var(--onejax-blue)",
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
        var crossSectorCollabs = _context.CrossSectorCollabs.ToList();

        // Only create goals if we have real data
        if (staffSurveys.Any() || profDev.Any() || mediaPlacements.Any() || websiteTraffic.Any() || crossSectorCollabs.Any())
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

            // Goal 4: Community Engagement (with CrossSector data if available)
            if (crossSectorCollabs.Any())
            {
                var communityGoal = CreateCommunityEngagementGoalFromRealData(crossSectorCollabs);
                goals.Add(communityGoal);
            }
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

    private StrategicGoal CreateCommunityEngagementGoalFromRealData(List<CrossSector10D> crossSectorCollabs)
    {
        var goal = new StrategicGoal
        {
            Id = 4,
            Name = "Community Engagement",
            Description = "Cross-sector partnerships and community collaborations",
            Color = "var(--onejax-blue)",
            Events = new List<Event>(),
            Metrics = new List<GoalMetric>()
        };

        // Calculate statistics
        var activeCollabs = crossSectorCollabs.Where(c => c.Status == "Active").ToList();
        var inactiveCollabs = crossSectorCollabs.Where(c => c.Status == "Inactive").ToList();
        var averageSatisfaction = crossSectorCollabs.Any() ? 
            crossSectorCollabs.Average(c => c.partner_satisfaction_ratings) : 0;

        // Add metrics based on CrossSector data
        goal.Metrics.Add(new GoalMetric
        {
            Id = 100,
            Name = "Total Cross-Sector Partnerships",
            Description = "Number of cross-sector collaborations established",
            StrategicGoalId = 4,
            Target = "25",
            CurrentValue = crossSectorCollabs.Count,
            Unit = "partnerships",
            Status = crossSectorCollabs.Count >= 25 ? "On Target" : "In Progress",
            TargetDate = DateTime.Now.AddMonths(12)
        });

        goal.Metrics.Add(new GoalMetric
        {
            Id = 101,
            Name = "Active Partnerships",
            Description = "Currently active cross-sector collaborations",
            StrategicGoalId = 4,
            Target = "15",
            CurrentValue = activeCollabs.Count,
            Unit = "active partnerships",
            Status = activeCollabs.Count >= 15 ? "On Target" : "In Progress",
            TargetDate = DateTime.Now.AddMonths(6)
        });

        goal.Metrics.Add(new GoalMetric
        {
            Id = 102,
            Name = "Partnership Satisfaction",
            Description = "Average partner satisfaction rating",
            StrategicGoalId = 4,
            Target = "80",
            CurrentValue = averageSatisfaction,
            Unit = "% satisfaction",
            Status = averageSatisfaction >= 80 ? "Excellent" : averageSatisfaction >= 60 ? "Good" : "Needs Improvement",
            TargetDate = DateTime.Now.AddMonths(3)
        });

        // Add recent partnerships as events
        var recentCollabs = crossSectorCollabs.OrderByDescending(c => c.CreatedDate).Take(5);
        foreach (var collab in recentCollabs)
        {
            goal.Events.Add(new Event
            {
                Id = 1000 + collab.Id,
                Title = collab.Name,
                Description = $"Cross-sector collaboration established with satisfaction rating of {collab.partner_satisfaction_ratings}%",
                StartDate = collab.CreatedDate,
                EndDate = new DateTime(collab.Year, 12, 31),
                Status = collab.Status,
                StrategicGoalId = 4,
                Type = "Partnership",
                Notes = collab.Notes ?? ""
            });
        }

        return goal;
    }

    private async Task<DashboardViewModel> BuildEnhancedDashboardAsync(string fiscalYear = "2025-2026")
    {
        var dashboard = new DashboardViewModel();
        
        // Always create all four strategic goals (regardless of data)
        var allGoals = await CreateAllStrategicGoalsAsync();
        
        // Enhance each strategic goal with comprehensive metrics from MetricsService
        await EnhanceGoalsWithComprehensiveMetricsAsync(allGoals, fiscalYear);
        
        // Build summary statistics from real data
        dashboard.Summary = BuildDashboardSummary();
        
        // Build recent activities from real data
        dashboard.RecentActivities = BuildRecentActivities();

        // Build chart data from real metrics
        dashboard.Charts = BuildChartData(allGoals);
        
        // Always show all goals with their metrics (even if empty)
        dashboard.StrategicGoals = allGoals;
        dashboard.DataSource = "Comprehensive Metrics + Real Data Integration";
        dashboard.Message = "Dashboard shows all strategic goals with comprehensive metrics. Metrics update in real-time as you submit data through the Data Entry forms.";
        
        return dashboard;
    }

    private async Task<List<StrategicGoal>> CreateAllStrategicGoalsAsync()
    {
        var goals = new List<StrategicGoal>();
        
        // Define the four strategic goals structure
        var goalTemplates = new List<(int Id, string Name, string Description, string Color)>
        {
            (1, "Organizational Building", "Strengthening organizational structure and capacity", "var(--onejax-navy)"),
            (2, "Financial Sustainability", "Ensuring sustainable financial health and growth", "var(--onejax-green)"),
            (3, "Identity/Value Proposition", "Establishing and communicating OneJax's unique identity and value", "var(--onejax-orange)"),
            (4, "Community Engagement", "Building partnerships and community connections", "var(--onejax-blue)")
        };

        foreach (var template in goalTemplates)
        {
            var goal = new StrategicGoal
            {
                Id = template.Id,
                Name = template.Name,
                Description = template.Description,
                Color = template.Color,
                Events = new List<Event>(),
                Metrics = new List<GoalMetric>()
            };
            
            // Add events from Strategy controller's static list
            goal.Events.AddRange(GetEventsFromStrategyController(template.Id));
            
            goals.Add(goal);
        }

        return goals;
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
            // Identity/Value Proposition Goal - Media, Website, Demographics, Planning
            if (goal.Name.Contains("Identity"))
            {
                await AddIdentityMetricsAsync(goal);
            }
        
            // Organizational Building Goal - Staff, Professional Development, Board Management
            else if (goal.Name.Contains("Organizational"))
            {
                await AddOrganizationalMetricsAsync(goal);
            }
        
            // Financial Sustainability Goal - Budget, Revenue, Donors, Fees
            else if (goal.Name.Contains("Financial"))
            {
                await AddFinancialMetricsAsync(goal);
            }

            // Community Engagement Goal - Collaborations, Communications, Surveys, Programs
            else if (goal.Name.Contains("Community"))
            {
                await AddCommunityMetricsAsync(goal);
            }

            // Save all metric updates to the database
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating metrics: {ex.Message}");
        }
    }

    private async Task AddIdentityMetricsAsync(StrategicGoal goal)
    {
        var nextId = goal.Metrics.Count + 1000;

        // 1. Media Placements
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
        
        AddOrUpdateMetric(goal, "Media Placements", "Professional media coverage tracking", 
            totalPlacements, "placements", "50", mediaPlacements.Any() ? "Active" : "Planning",
            mediaPlacements.Any() ? $"Total media placements: {totalPlacements}" : "No media placements yet - Go to Data Entry → Media Placements", nextId++);

        // 2. Website Traffic
        var websiteTraffic = await _context.WebsiteTraffic.ToListAsync();
        var totalTraffic = websiteTraffic.Sum(w => (w.Q1_JulySeptember ?? 0) + (w.Q2_OctoberDecember ?? 0) + 
                                                  (w.Q3_JanuaryMarch ?? 0) + (w.Q4_AprilJune ?? 0));
        
        AddOrUpdateMetric(goal, "Website Traffic (Annual)", "Total website clicks per year", 
            totalTraffic, "clicks", "10000", websiteTraffic.Any() ? "Active" : "Planning",
            websiteTraffic.Any() ? $"Total annual traffic: {totalTraffic:N0} clicks" : "No website traffic data yet - Go to Data Entry → Website Traffic", nextId++);

        // 3. Demographics Expansion
        var demographics = await _context.demographics_8D.ToListAsync();
        var uniqueZipCodes = 0;
        if (demographics.Any())
        {
            uniqueZipCodes = demographics.SelectMany(d => d.ZipCodes.Split(',', StringSplitOptions.RemoveEmptyEntries))
                                       .Select(z => z.Trim()).Distinct().Count();
        }
        
        AddOrUpdateMetric(goal, "Geographic Reach", "Unique ZIP codes served", 
            uniqueZipCodes, "ZIP codes", "25", demographics.Any() ? "Active" : "Planning",
            demographics.Any() ? $"Program reach: {uniqueZipCodes} unique ZIP codes" : "No demographics data yet - Go to Data Entry → Demographics", nextId++);

        // 4. Strategic Planning Issues
        var planIssues = await _context.planIssue_25D.ToListAsync();
        
        AddOrUpdateMetric(goal, "Strategic Issues Tracking", "Issues identified for planning", 
            planIssues.Count, "issues", "15", planIssues.Any() ? "Active" : "Planning",
            planIssues.Any() ? $"Issues tracked: {planIssues.Count}" : "No planning issues yet - Go to Data Entry → Planning Issues", nextId++);

        // 5. 2026 Planning
        var plan2026 = await _context.Plan2026_24D.ToListAsync();
        
        AddOrUpdateMetric(goal, "2026 Strategic Plans", "Future planning initiatives", 
            plan2026.Count, "plans", "20", plan2026.Any() ? "Active" : "Planning",
            plan2026.Any() ? $"Plans for 2026: {plan2026.Count}" : "No 2026 plans yet - Go to Data Entry → 2026 Planning", nextId++);
    }

    private async Task AddOrganizationalMetricsAsync(StrategicGoal goal)
    {
        var nextId = goal.Metrics.Count + 2000;

        // 1. Staff Surveys
        var staffSurveys = await _context.StaffSurveys_22D.ToListAsync();
        var avgSatisfaction = staffSurveys.Any() ? staffSurveys.Average(s => s.SatisfactionRate) : 0;
        
        AddOrUpdateMetric(goal, "Staff Survey Responses", "Employee feedback collection", 
            staffSurveys.Count, "responses", "25", staffSurveys.Any() ? "Active" : "Planning",
            staffSurveys.Any() ? $"{staffSurveys.Count} responses, {avgSatisfaction:F1}% avg satisfaction" : "No staff surveys yet - Go to Data Entry → Staff Surveys", nextId++);

        // 2. Professional Development
        var profDevs = await _context.ProfessionalDevelopments.ToListAsync();
        var totalDevelopment = profDevs.Sum(p => p.ProfessionalDevelopmentYear26 + p.ProfessionalDevelopmentYear27);
        
        AddOrUpdateMetric(goal, "Professional Development Plans", "Staff growth initiatives", 
            profDevs.Count, "plans", "30", profDevs.Any() ? "Active" : "Planning",
            profDevs.Any() ? $"{profDevs.Count} development plans, {totalDevelopment} total activities" : "No professional development yet - Go to Data Entry → Professional Development", nextId++);

        // 3. Board Member Recruitment
        var boardMembers = await _context.BoardMember_29D.ToListAsync();
        var totalRecruited = boardMembers.Sum(b => b.NumberRecruited);
        
        AddOrUpdateMetric(goal, "Board Recruitment", "New board member acquisition", 
            totalRecruited, "members", "10", boardMembers.Any() ? "Active" : "Planning",
            boardMembers.Any() ? $"Board members recruited: {totalRecruited}" : "No board recruitment data yet - Go to Data Entry → Board Management", nextId++);

        // 4. Board Meeting Attendance
        var boardAttendance = await _context.BoardMeetingAttendance.ToListAsync();
        var avgAttendance = boardAttendance.Any() ? boardAttendance.Average(b => b.MembersInAttendance) : 0;
        
        AddOrUpdateMetric(goal, "Board Meeting Participation", "Average meeting attendance", 
            Math.Round((decimal)avgAttendance, 1), "members", "12", boardAttendance.Any() ? "Active" : "Planning",
            boardAttendance.Any() ? $"Average attendance: {avgAttendance:F1} members" : "No board attendance data yet - Go to Data Entry → Board Management", nextId++);
    }

    private async Task AddFinancialMetricsAsync(StrategicGoal goal)
    {
        var nextId = goal.Metrics.Count + 3000;

        // 1. Budget Tracking
        var budgetTracking = await _context.BudgetTracking_28D.ToListAsync();
        decimal totalRevenue = 0;
        decimal totalExpenses = 0;
        
        foreach (var budget in budgetTracking)
        {
            // Revenue fields only
            totalRevenue += (budget.CorporateGiving ?? 0) + (budget.IndividualGiving ?? 0) + 
                           (budget.GrantsFoundations ?? 0) + (budget.CommunityEvents ?? 0) + 
                           (budget.PeopleCultureWorkshops ?? 0);
            
            // Expense fields only  
            totalExpenses += (budget.CommunityPrograms ?? 0) + (budget.OneYouthPrograms ?? 0) + 
                            (budget.InterfaithPrograms ?? 0) + (budget.HumanitarianEvent ?? 0);
        }
        
        AddOrUpdateMetric(goal, "Budget Revenue Tracking", "Total tracked revenue streams", 
            totalRevenue, "dollars", "500000", budgetTracking.Any() ? "Active" : "Planning",
            budgetTracking.Any() ? $"Total revenue: ${totalRevenue:N0}, Total expenses: ${totalExpenses:N0}" : "No budget data yet - Go to Data Entry → Budget Tracking", nextId++);
            
        AddOrUpdateMetric(goal, "Budget Expense Tracking", "Total tracked expense streams", 
            totalExpenses, "dollars", "400000", budgetTracking.Any() ? "Active" : "Planning",
            budgetTracking.Any() ? $"Total expenses: ${totalExpenses:N0}" : "No budget expense data yet", nextId++);

        // 2. Fee-for-Service Revenue
        var feeServices = await _context.FeeForServices_21D.ToListAsync();
        var totalFeeRevenue = feeServices.Sum(f => f.RevenueReceived);
        
        AddOrUpdateMetric(goal, "Fee-for-Service Income", "Service-based revenue generation", 
            totalFeeRevenue, "dollars", "75000", feeServices.Any() ? "Active" : "Planning",
            feeServices.Any() ? $"Service revenue: ${totalFeeRevenue:N0} from {feeServices.Count} services" : "No fee-for-service data yet - Go to Data Entry → Fee for Services", nextId++);

        // 3. Income Tracking
        var incomeData = await _context.income_27D.ToListAsync();
        var totalIncome = incomeData.Sum(i => i.Amount);
        
        AddOrUpdateMetric(goal, "General Income Streams", "Diversified income tracking", 
            totalIncome, "dollars", "100000", incomeData.Any() ? "Active" : "Planning",
            incomeData.Any() ? $"Total income: ${totalIncome:N0} from {incomeData.Count} sources" : "No income data yet - Go to Data Entry → Income Tracking", nextId++);

        // 4. Donor Events
        var donorEvents = await _context.DonorEvents_19D.ToListAsync();
        var totalParticipants = donorEvents.Sum(d => d.NumberOfParticipants);
        var avgSatisfaction = donorEvents.Any() ? donorEvents.Average(d => d.EventSatisfactionRating) : 0;
        
        AddOrUpdateMetric(goal, "Donor Engagement Events", "Fundraising event effectiveness", 
            totalParticipants, "participants", "200", donorEvents.Any() ? "Active" : "Planning",
            donorEvents.Any() ? $"{totalParticipants} participants, {avgSatisfaction:F1}/5 avg satisfaction" : "No donor events yet - Go to Data Entry → Donor Events", nextId++);
    }

    private async Task AddCommunityMetricsAsync(StrategicGoal goal)
    {
        var nextId = goal.Metrics.Count + 4000;

        // 1. Cross-Sector Collaborations
        var crossSectorCollabs = await _context.CrossSectorCollabs.ToListAsync();
        var activeCollabs = crossSectorCollabs.Count(c => c.Status == "Active");
        
        AddOrUpdateMetric(goal, "Cross-Sector Partnerships", "Strategic community alliances", 
            crossSectorCollabs.Count, "partnerships", "20", crossSectorCollabs.Any() ? "Active" : "Planning",
            crossSectorCollabs.Any() ? $"{crossSectorCollabs.Count} partnerships ({activeCollabs} active)" : "No cross-sector collaborations yet - Go to Data Entry → Cross-Sector Collaborations", nextId++);

        // 2. Communication Rate
        var commRate = await _context.CommunicationRate.ToListAsync();
        
        AddOrUpdateMetric(goal, "Community Communications", "Outreach and engagement tracking", 
            commRate.Count, "communications", "100", commRate.Any() ? "Active" : "Planning",
            commRate.Any() ? $"Communication entries: {commRate.Count}" : "No communication data yet - Go to Data Entry → Communications", nextId++);

        // 3. Annual Community Survey
        var annualSurvey = await _context.Annual_average_7D.ToListAsync();
        var latestSurvey = annualSurvey.OrderByDescending(s => s.Year).FirstOrDefault();
        var trustRating = latestSurvey?.Percentage ?? 0;
        
        AddOrUpdateMetric(goal, "Community Trust Rating", "Annual community perception survey", 
            trustRating, "percent", "80", annualSurvey.Any() ? "Active" : "Planning",
            annualSurvey.Any() ? $"{trustRating}% trust rating ({latestSurvey?.TotalRespondents} respondents, {latestSurvey?.Year})" : "No community survey data yet - Go to Data Entry → Annual Survey", nextId++);
    }

    private void AddOrUpdateMetric(StrategicGoal goal, string name, string description, decimal currentValue, 
                                 string unit, string target, string status, string detailedDescription, int id)
    {
        var existingMetric = goal.Metrics.FirstOrDefault(m => m.Name == name);
        
        if (existingMetric != null)
        {
            // Update existing metric
            existingMetric.CurrentValue = currentValue;
            existingMetric.Status = status;
            existingMetric.Description = detailedDescription;
            existingMetric.Target = target;
        }
        else
        {
            // Create new metric
            goal.Metrics.Add(new GoalMetric
            {
                Id = id,
                Name = name,
                Description = detailedDescription,
                StrategicGoalId = goal.Id,
                Target = target,
                CurrentValue = currentValue,
                Unit = unit,
                DataSource = "Form",
                MetricType = "Count",
                IsPublic = true,
                FiscalYear = "2025-2026",
                Status = status,
                TargetDate = DateTime.Now.AddMonths(12)
            });
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
            return goal.Metrics.Any() ? 15 : 0; // Lower default progress if we have metrics but no targets

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

    private decimal ParseCurrency(string currencyString)
    {
        if (string.IsNullOrWhiteSpace(currencyString))
            return 0m;

        // Remove currency symbols, commas, and extra spaces
        var cleaned = currencyString.Trim()
            .Replace("$", "")
            .Replace(",", "")
            .Replace(" ", "");

        return decimal.TryParse(cleaned, out decimal result) ? result : 0m;
    }
}
