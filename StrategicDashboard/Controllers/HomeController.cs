using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneJax.StrategicDashboard.Models;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using OneJaxDashboard.Services;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
//emily
public class HomeController : Controller
{
    private const string DefaultFiscalYear = "2025-2026";
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
            var selectedFiscalYear = string.IsNullOrWhiteSpace(fiscalYear) ? DefaultFiscalYear : fiscalYear;
            var appliedFiscalYearFilter = string.IsNullOrWhiteSpace(fiscalYear) ? "" : fiscalYear;

            // Create enhanced dashboard data with comprehensive metrics
            var dashboardData = await BuildEnhancedDashboardAsync(selectedFiscalYear);
            
            // Apply filters
            dashboardData = ApplyFilters(dashboardData, status ?? "", time ?? "", goal ?? "", appliedFiscalYearFilter, quarter ?? "");

            // Community Engagement visuals (dashboard only): Youth Attendance + Partner Touchpoints charts/tables.
            // Keep public safe: aggregate values only, and latest-entries table excludes partner/contact PII.
            try
            {
                (DateTime StartDate, DateTime EndDate)? selectedFiscalYearRange = null;
                if (TryParseFiscalYearEnd(selectedFiscalYear, out var selectedFiscalYearEnd))
                {
                    selectedFiscalYearRange = GetFiscalYearRange(selectedFiscalYearEnd);
                }

                // Youth Attendance (YouthAttend_15D): line chart of attendees per event over time + avg pre/post assessment bars.
                var youthRows = await _context.YouthAttend_15D
                    .Include(y => y.Strategy)
                    .OrderBy(y => y.CreatedDate)
                    .ToListAsync();

                if (selectedFiscalYearRange != null)
                {
                    youthRows = youthRows
                        .Where(y => y.CreatedDate >= selectedFiscalYearRange.Value.StartDate && y.CreatedDate <= selectedFiscalYearRange.Value.EndDate)
                        .ToList();
                }

                var youthRecent = youthRows
                    .OrderBy(y => y.CreatedDate)
                    .TakeLast(20)
                    .ToList();

                ViewBag.CommunityYouthAttendanceSeries = youthRecent
                    .Select(y => new
                    {
                        label = y.CreatedDate.ToString("MMM d", CultureInfo.InvariantCulture),
                        attendees = y.NumberOfYouthAttendees
                    })
                    .ToList();

                ViewBag.CommunityYouthPrePostAvg = new
                {
                    pre = youthRows.Any() ? Math.Round(youthRows.Average(y => (double)y.AveragePreAssessment), 1) : 0.0,
                    post = youthRows.Any() ? Math.Round(youthRows.Average(y => (double)y.AveragePostAssessment), 1) : 0.0
                };

                // Collaborative Partner Touchpoints (CollabTouch_47D): FY bar chart + latest 5 table (date + strategy only).
                var collabRows = await _context.CollabTouch_47D
                    .Include(c => c.Strategy)
                    .OrderByDescending(c => c.CreatedDate)
                    .ToListAsync();

                if (selectedFiscalYearRange != null)
                {
                    collabRows = collabRows
                        .Where(c => c.CreatedDate >= selectedFiscalYearRange.Value.StartDate && c.CreatedDate <= selectedFiscalYearRange.Value.EndDate)
                        .ToList();
                }

                ViewBag.CommunityPartnersByFiscalYear = collabRows
                    .GroupBy(c => string.IsNullOrWhiteSpace(c.FiscalYear) ? "Unknown" : c.FiscalYear.Trim())
                    .OrderBy(g => g.Key)
                    .Select(g => new { fiscalYear = g.Key, count = g.Count() })
                    .ToList();

                ViewBag.CommunityPartnersLatest = collabRows
                    .OrderByDescending(c => c.CreatedDate)
                    .Take(5)
                    .Select(c => new
                    {
                        date = c.CreatedDate.ToString("MMM d, yyyy", CultureInfo.InvariantCulture),
                        strategy = c.Strategy?.Name ?? "Unassigned"
                    })
                    .ToList();
            }
            catch { }

            // Add board meeting attendance records for the enhanced view
            ViewBag.BoardMeetingAttendance = await _context.BoardMeetingAttendance
                .OrderByDescending(b => b.MeetingDate)
                .Take(10) // Limit to last 10 meetings
                .ToListAsync();

            // Add budget tracking data for Financial Sustainability chart
            var budgetTracking = await _context.BudgetTracking_28D.ToListAsync();
            if (TryParseFiscalYearEnd(appliedFiscalYearFilter, out var budgetFiscalYearEnd))
            {
                var budgetFiscalYearStart = budgetFiscalYearEnd - 1;
                var fiscalYearBudgetTracking = budgetTracking
                    .Where(b => b.Year == budgetFiscalYearStart || b.Year == budgetFiscalYearEnd)
                    .ToList();
                budgetTracking = fiscalYearBudgetTracking.Any() ? fiscalYearBudgetTracking : budgetTracking;
            }
            ViewBag.BudgetTracking = budgetTracking;

            // Add volunteer program records for detailed organizational card display
            ViewBag.VolunteerProgramRecords = await _context.volunteerProgram_40D
                .OrderByDescending(v => v.Year)
                .ThenByDescending(v => v.Quarter)
                .ThenByDescending(v => v.CreatedDate)
                .Take(12)
                .ToListAsync();

            return View(dashboardData);
        }
        catch (Exception ex)
        {
            // Enhanced error logging for Azure diagnostics
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<HomeController>>();
            logger.LogError(ex, "Dashboard data loading failed. FiscalYear: {FiscalYear}, Goal: {Goal}", fiscalYear, goal);
            
            // Show error message but with empty data structure
            var errorData = new DashboardViewModel 
            { 
                StrategicGoals = new List<StrategicGoal>(),
                HasError = true,
                ErrorMessage = ex.InnerException?.Message ?? ex.Message,
                DataSource = "Database Connection Error"
            };
            
            return View(errorData);
        }
    }

    private DashboardViewModel ApplyFilters(DashboardViewModel dashboard, string status, string time, string goal, string fiscalYear, string quarter)
    {
        // Do not filter StrategicGoals by `goal` query param.
        // The selected tab is handled in the view/client, and filtering here causes
        // a refresh to render only one tab.

        // Apply time-based filters (quarter/legacy time only)
        var timeFilter = GetTimeFilter(time, fiscalYear, quarter);
        var fiscalYearRange = TryParseFiscalYearEnd(fiscalYear, out int fiscalYearEnd)
            ? GetFiscalYearRange(fiscalYearEnd)
            : ((DateTime StartDate, DateTime EndDate)?)null;

        foreach (var g in dashboard.StrategicGoals)
        {
            if (g.Events != null)
            {
                g.Events = g.Events
                    .Where(e => 
                    {
                        // Status filter
                        var statusMatch = string.IsNullOrEmpty(status) || e.Status == status;
                        
                        // Legacy/quarter time filter
                        var timeMatch = timeFilter == null || 
                                       (e.DueDate >= timeFilter.Value.StartDate && e.DueDate <= timeFilter.Value.EndDate);

                        // Fiscal year filter (events only). Keep events without dates visible.
                        var fiscalYearMatch = fiscalYearRange == null
                            || !e.DueDate.HasValue
                            || (e.DueDate.Value >= fiscalYearRange.Value.StartDate && e.DueDate.Value <= fiscalYearRange.Value.EndDate);
                        
                        return statusMatch && timeMatch && fiscalYearMatch;
                    })
                    .ToList();
            }

            // Apply only explicit time filters (quarter/legacy) to metrics.
            // Fiscal year for metrics is handled by MetricsService.GetPublicMetricsAsync(..., fiscalYear).
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
        
        // If specific fiscal year and quarter are provided (legacy support)
        if (!string.IsNullOrEmpty(fiscalYear) && !string.IsNullOrEmpty(quarter))
        {
            if (TryParseFiscalYearEnd(fiscalYear, out int year) && int.TryParse(quarter.Replace("Q", ""), out int q))
            {
                return GetFiscalQuarterRange(year, q);
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

    private bool TryParseFiscalYearEnd(string fiscalYear, out int fiscalYearEnd)
    {
        fiscalYearEnd = 0;
        if (string.IsNullOrWhiteSpace(fiscalYear))
        {
            return false;
        }

        var normalized = fiscalYear.Trim();
        if (normalized.StartsWith("FY ", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized.Substring(3).Trim();
        }

        // Preferred format: "2025-2026" (or with "/")
        var separators = new[] { '-', '/' };
        var parts = normalized.Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 2
            && int.TryParse(parts[0], out var startYear)
            && int.TryParse(parts[1], out var endYear)
            && endYear == startYear + 1)
        {
            fiscalYearEnd = endYear;
            return true;
        }

        // Legacy fallback: "2026"
        return int.TryParse(normalized, out fiscalYearEnd);
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
        var allGoals = generatedGoals;

        // Build summary statistics from real data
        dashboard.Summary = BuildDashboardSummary();
        
        // Build recent activities from real data
        dashboard.RecentActivities = BuildRecentActivities();

        // Build chart data from real metrics
        dashboard.Charts = BuildChartData(allGoals);
        
        if (allGoals.Any(g => g.Metrics.Any() || g.Events.Any()))
        {
            dashboard.StrategicGoals = allGoals;
            dashboard.DataSource = "Real Data Entries";
            dashboard.Message =
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

    private async Task<List<Event>> GetDashboardEventsForGoalAsync(int strategicGoalId)
    {
        return await _context.Events
            .Where(e => !e.IsArchived && e.StrategyId.HasValue)
            .Join(
                _context.Strategies,
                e => e.StrategyId,
                s => (int?)s.Id,
                (e, s) => new { Event = e, Strategy = s }
            )
            .Where(joined => joined.Strategy.StrategicGoalId == strategicGoalId)
            .Select(joined => joined.Event)
            .OrderBy(e => e.DueDate == null)
            .ThenBy(e => e.DueDate)
            .Take(12)
            .ToListAsync();
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

            // Events (real Events table only)
            try
            {
                summary.TotalEvents = _context.Events.Count(e => !e.IsArchived);
            }
            catch
            {
                // Fallback if database access fails
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
                    Description = $"Staff satisfaction survey completed (Rate: {survey.SatisfactionRate}%)",
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
                    Description = $"{profDev.Name} submitted professional development plan",
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

            // Add recent events from database Events table
            try 
            {
                var recentEvents = _context.Events
                    .OrderByDescending(e => e.StartDate ?? e.DueDate ?? DateTime.Now)
                    .Take(3)
                    .ToList();

                foreach (var evt in recentEvents)
                {
                    var goalId = ResolveGoalIdForEvent(evt) ?? 1;
                    activities.Add(new RecentActivity
                    {
                        Type = "Event",
                        Title = evt.Title,
                        Description = $"{evt.Type} | Status: {evt.Status} | {(evt.StartDate?.ToString("MMM dd") ?? evt.DueDate?.ToString("MMM dd") ?? "Date TBD")}",
                        Date = evt.StartDate ?? evt.DueDate ?? DateTime.Now,
                        Icon = "fas fa-calendar-check",
                        Color = GetColorByGoalId(goalId),
                        GoalName = GetGoalNameById(goalId)
                    });
                }
            }
            catch (Exception)
            {
                // Events table might not be available yet
            }

            // Add recent Core Strategy events
            try 
            {
                var recentStrategies = _context.Strategies
                    .OrderByDescending(s => s.Id)
                    .Take(3)
                    .ToList();

                foreach (var strategy in recentStrategies)
                {
                    activities.Add(new RecentActivity
                    {
                        Type = "Core Strategy Event",
                        Title = strategy.Name,
                        Description = $"{(!string.IsNullOrWhiteSpace(strategy.ProgramName) ? strategy.ProgramName : (!string.IsNullOrWhiteSpace(strategy.ProgramType) ? strategy.ProgramType : "Program"))} | {(DateTime.TryParse(strategy.Date, out var date) ? date.ToString("MMM dd") : "Date TBD")} | {(!string.IsNullOrEmpty(strategy.EventFYear) ? $"FY {strategy.EventFYear}" : "")}",
                        Date = DateTime.TryParse(strategy.Date, out var strategyDate) ? strategyDate : DateTime.Now.AddDays(-1),
                        Icon = "fas fa-calendar-plus",
                        Color = GetColorByGoalId(strategy.StrategicGoalId),
                        GoalName = GetGoalNameById(strategy.StrategicGoalId)
                    });
                }
            }
            catch (Exception)
            {
                // Strategies table might not be available yet
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

        // Generate metrics from staff surveys (always create metrics, default to 0 if no data)
        var avgSatisfaction = staffSurveys.Any() ? staffSurveys.Average(s => s.SatisfactionRate) : 0;
        var totalStaff = staffSurveys.Count;
        
        goal.Metrics.Add(new GoalMetric
        {
            Id = 1,
            Name = "Staff Satisfaction Rate",
            Description = totalStaff > 0 ? $"Based on {totalStaff} staff survey responses" : "No survey data available - showing baseline",
            StrategicGoalId = 1,
            Target = "90",
            CurrentValue = (decimal)Math.Round(avgSatisfaction, 1),
            Unit = "%",
            Status = totalStaff > 0 ? "In Progress" : "Pending Data",
            TargetDate = DateTime.Now.AddMonths(12)
        });

        // Generate metrics from professional development data (always create metrics, default to 0 if no data)
        var totalDevelopmentRecords = profDev.Count();
        goal.Metrics.Add(new GoalMetric
        {
            Id = 2,
            Name = "Professional Development Planning",
            Description = profDev.Any() ? $"Professional development records submitted" : "No planning data available - showing baseline",
            StrategicGoalId = 1,
            Target = "",
            CurrentValue = totalDevelopmentRecords,
            Unit = "records",
            Status = profDev.Any() ? "In Progress" : "Pending Data",
            TargetDate = DateTime.Now.AddMonths(12)
        });

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

        // Identity: ZIP coverage (drives Programs Demographics map)
        dashboard.ZipCoverage = await BuildZipCoverageAsync();
        dashboard.Identity = await BuildIdentityDashboardDataAsync(dashboard.ZipCoverage);
        
        return dashboard;
    }

    private async Task<IdentityDashboardData> BuildIdentityDashboardDataAsync(Dictionary<string, int> zipCoverage)
    {
        var data = new IdentityDashboardData
        {
            ZipCodesServed = zipCoverage?.Count ?? 0
        };

        // Media Placements
        try
        {
            var all = await _context.MediaPlacements_3D.ToListAsync();
            data.MediaPlacementsTotal = all.Sum(m => m.TotalMentions);
            data.MediaPlacementsByMonth = new[]
            {
                all.Sum(m => m.January ?? 0),
                all.Sum(m => m.February ?? 0),
                all.Sum(m => m.March ?? 0),
                all.Sum(m => m.April ?? 0),
                all.Sum(m => m.May ?? 0),
                all.Sum(m => m.June ?? 0),
                all.Sum(m => m.July ?? 0),
                all.Sum(m => m.August ?? 0),
                all.Sum(m => m.September ?? 0),
                all.Sum(m => m.October ?? 0),
                all.Sum(m => m.November ?? 0),
                all.Sum(m => m.December ?? 0)
            };
            data.MediaPlacementsLastUpdated = all.Count > 0 ? all.Max(m => m.CreatedDate) : null;
        }
        catch { }

        // Website Traffic
        try
        {
            var all = await _context.WebsiteTraffic.ToListAsync();
            data.WebsiteClicksTotal = all.Sum(w => w.TotalClicks);
            data.WebsiteTrafficLastUpdated = all.Count > 0 ? all.Max(w => w.CreatedDate) : null;

            var latest = all
                .OrderByDescending(w => w.CreatedDate)
                .FirstOrDefault();
            if (latest != null)
            {
                data.WebsiteClicksByQuarter = new[]
                {
                    latest.Q1_JulySeptember ?? 0,
                    latest.Q2_OctoberDecember ?? 0,
                    latest.Q3_JanuaryMarch ?? 0,
                    latest.Q4_AprilJune ?? 0
                };
            }
        }
        catch { }

        // Community Perception Survey (Trust)
        try
        {
            var latest = await _context.Annual_average_7D
                .OrderByDescending(s => s.Year)
                .ThenByDescending(s => s.CreatedDate)
                .FirstOrDefaultAsync();
            data.TrustPercent = latest?.Percentage ?? 0m;
            data.TrustRespondents = latest?.TotalRespondents;
            data.TrustYear = latest?.Year;
            data.TrustLastUpdated = latest?.CreatedDate;

            var history = await _context.Annual_average_7D
                .OrderByDescending(s => s.Year)
                .ThenByDescending(s => s.CreatedDate)
                .Take(5)
                .ToListAsync();
            history = history
                .GroupBy(h => h.Year)
                .Select(g => g.OrderByDescending(x => x.CreatedDate).First())
                .OrderBy(h => h.Year)
                .ToList();

            data.TrustHistoryYears = history.Select(h => h.Year).ToList();
            data.TrustHistoryPercents = history.Select(h => h.Percentage).ToList();
        }
        catch { }

        // Milestone Achievement
        try
        {
            var latest = await _context.achieveMile_6D
                .OrderByDescending(m => m.CreatedDate)
                .FirstOrDefaultAsync();
            data.MilestonePercent = latest?.Percentage ?? 0m;
            data.MilestoneReviewActive = latest?.achievedReview ?? false;
            data.MilestoneLastUpdated = latest?.CreatedDate;
        }
        catch { }

        // Social Media Engagement
        try
        {
            var latest = await _context.socialMedia_5D
                .OrderByDescending(s => s.Year)
                .ThenByDescending(s => s.CreatedDate)
                .FirstOrDefaultAsync();
            if (latest != null)
            {
                data.SocialYear = latest.Year;
                data.SocialAvgEngagementRate = latest.AverageEngagementRate;
                data.SocialQ1 = latest.JulySeptEngagementRate;
                data.SocialQ2 = latest.OctDecEngagementRate;
                data.SocialQ3 = latest.JanMarEngagementRate;
                data.SocialQ4 = latest.AprilJuneEngagementRate;
                data.SocialGoalMet = latest.GoalMet;
                data.SocialLastUpdated = latest.CreatedDate;
            }
        }
        catch { }

        // Framework Development Plan
        try
        {
            var latest = await _context.Plan2026_24D
                .OrderByDescending(p => p.Year)
                .ThenByDescending(p => p.Quarter)
                .ThenByDescending(p => p.CreatedDate)
                .FirstOrDefaultAsync();
            if (latest != null)
            {
                data.FrameworkYear = latest.Year;
                data.FrameworkQuarter = latest.Quarter ?? "";
                data.FrameworkStatus = latest.FrameworkStatus ?? "";
                data.FrameworkGoalMet = latest.GoalMet;
                data.FrameworkLastUpdated = latest.CreatedDate;
            }
        }
        catch { }

        return data;
    }

    private async Task<Dictionary<string, int>> BuildZipCoverageAsync()
    {
        var coverage = new Dictionary<string, int>(StringComparer.Ordinal);

        List<demographics_8D> rows;
        try
        {
            rows = await _context.demographics_8D
                .OrderByDescending(d => d.CreatedDate)
                .ToListAsync();
        }
        catch
        {
            return coverage;
        }

        foreach (var row in rows)
        {
            var zipCodes = (row.ZipCodes ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var raw in zipCodes)
            {
                var digits = new string(raw.Where(char.IsDigit).ToArray());
                if (digits.Length < 5) continue;
                var zip = digits.Substring(0, 5);

                if (coverage.TryGetValue(zip, out var c)) coverage[zip] = c + 1;
                else coverage[zip] = 1;
            }
        }

        return coverage;
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

            goal.Events = await GetDashboardEventsForGoalAsync(template.Id);
            
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
            
            // Identity/Value is rendered with custom visual cards on the dashboard (and derives values from the
            // Data Entry tables). Avoid injecting the seeded placeholder metrics here, which creates duplicates
            // and makes the tab look like a list of forms.
            if (goal.Name.Contains("Identity"))
                comprehensiveMetrics = new List<GoalMetric>();
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
            await UpdateMetricsWithRealDataAsync(goal, fiscalYear);
        }
    }
    
    private async Task UpdateMetricsWithRealDataAsync(StrategicGoal goal, string fiscalYear)
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
                await AddFinancialMetricsAsync(goal, fiscalYear);
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
            if (goal.Name.Contains("Financial"))
            {
                EnsureFinancialFallbackMetrics(goal);
            }
        }
    }

    private void EnsureFinancialFallbackMetrics(StrategicGoal goal)
    {
        var nextId = goal.Metrics.Count + 9000;

        AddMetricIfMissing(goal, "Budget Revenue Tracking", "Fallback metric while financial data loads",
            0, "dollars", "500000", "Planning", "Fallback: no budget tracking data available.", nextId++);

        AddMetricIfMissing(goal, "Fee-for-Service Income", "Fallback metric while earned income data loads",
            0, "dollars", "75000", "Planning", "Fallback: no fee-for-service data available.", nextId++);

        AddMetricIfMissing(goal, "General Income Streams", "Fallback metric while income data loads",
            0, "dollars", "100000", "Planning", "Fallback: no income tracking data available.", nextId++);

        AddMetricIfMissing(goal, "Donor Engagement Events", "Fallback metric while donor event data loads",
            0, "participants", "200", "Planning", "Fallback: no donor engagement data available.", nextId++);

        AddMetricIfMissing(goal, "Donor Communication Satisfaction", "Fallback metric while communication data loads",
            0, "%", "85", "Planning", "Fallback: no donor communication data available.", nextId++);
    }

    private void AddMetricIfMissing(StrategicGoal goal, string name, string description, decimal currentValue,
        string unit, string target, string status, string detailedDescription, int id)
    {
        if (goal.Metrics.Any(m => m.Name == name))
        {
            return;
        }

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

    private int? GetIncomeRecordYear(income_27D income)
    {
        if (!string.IsNullOrWhiteSpace(income.Month))
        {
            var parts = income.Month.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0 && int.TryParse(parts[^1], out var yearFromMonth))
            {
                return yearFromMonth;
            }
        }

        return income.Year;
    }

    private async Task AddIdentityMetricsAsync(StrategicGoal goal)
    {
        var nextId = goal.Metrics.Count + 1000;

        // IDENTITY/VALUE PROPOSITION - Connect ALL relevant forms
        
        // 1. Media Placements (from MediaPlacements_3D table)
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
        
        AddOrUpdateMetric(goal, "Earned Media Placements", "Professional media coverage tracking", 
            totalPlacements, "placements", "50", mediaPlacements.Any() ? "Active" : "Planning",
            $"📺 Media Placements: {totalPlacements} | Form: Data Entry → Media Placements", nextId++);

        // 2. Website Traffic (from WebsiteTraffic table - FIXED table name)
        var websiteTraffic = await _context.WebsiteTraffic.ToListAsync();
        var totalTraffic = websiteTraffic.Sum(w => (w.Q1_JulySeptember ?? 0) + (w.Q2_OctoberDecember ?? 0) + 
                                                  (w.Q3_JanuaryMarch ?? 0) + (w.Q4_AprilJune ?? 0));
        
        AddOrUpdateMetric(goal, "Website Traffic", "Quarterly website engagement", 
            totalTraffic, "clicks", "10000", websiteTraffic.Any() ? "Active" : "Planning",
            $"🌐 Website Traffic: {totalTraffic:N0} clicks | Form: Data Entry → Website Traffic", nextId++);

        // 3. Geographic Reach (from demographics_8D)
        var demographics = await _context.demographics_8D.ToListAsync();
        var uniqueZipCodes = 0;
        if (demographics.Any())
        {
            var allZipCodes = demographics.SelectMany(d => d.ZipCodes.Split(',', StringSplitOptions.RemoveEmptyEntries))
                                        .Select(z => z.Trim()).Where(z => !string.IsNullOrEmpty(z)).Distinct();
            uniqueZipCodes = allZipCodes.Count();
        }
        
        AddOrUpdateMetric(goal, "Geographic Reach", "Service area expansion", 
            uniqueZipCodes, "ZIP codes", "25", demographics.Any() ? "Active" : "Planning",
            $"📍 ZIP Codes Served: {uniqueZipCodes} | Form: Data Entry → Demographics", nextId++);

        // Note: Brand Trust Rate moved to Community Engagement tab as "Community Trust Rating"
        // to avoid duplication and better align with data source context

        // 4. Community Perception Survey (from Annual_average_7D)
        var annualSurvey = await _context.Annual_average_7D.ToListAsync();
        var latestSurvey = annualSurvey.OrderByDescending(s => s.Year).FirstOrDefault();
        var trustRating = latestSurvey?.Percentage ?? 0;
        
        AddOrUpdateMetric(goal, "Community Perception Survey", "Biannual survey - 70% trust rating target by Q4 2025", 
            trustRating, "%", "70", annualSurvey.Any() ? "Active" : "Planning",
            $"🌟 {trustRating}% identify OneJax as trusted leader ({latestSurvey?.TotalRespondents} respondents, {latestSurvey?.Year}) | Form: Data Entry → Annual Survey", nextId++);

        // 5. Strategic Planning (from Plan2026_24D)
        var plan2026 = await _context.Plan2026_24D.ToListAsync();
        var completedPlans = plan2026.Count(p => p.GoalMet);
        
        AddOrUpdateMetric(goal, "Strategic Plan Completion", "2026 planning progress", 
            completedPlans, "goals met", "20", plan2026.Any() ? "Active" : "Planning",
            $"🎯 Plans Completed: {completedPlans}/{plan2026.Count} | Form: Data Entry → 2026 Planning", nextId++);
    }

    private async Task AddOrganizationalMetricsAsync(StrategicGoal goal)
    {
        var nextId = goal.Metrics.Count + 2000;

        // 1. Staff Satisfaction (from Staff Surveys form)
        var staffSurveys = await _context.StaffSurveys_22D.ToListAsync();
        var avgSatisfaction = staffSurveys.Any() ? staffSurveys.Average(s => s.SatisfactionRate) : 0;
        
        AddOrUpdateMetric(goal, "Staff Satisfaction Rating", "Annual Team Satisfaction Survey", 
            Math.Round((decimal)avgSatisfaction, 1), "%", "85", staffSurveys.Any() ? "Active" : "Planning",
            staffSurveys.Any()
                ? $"{staffSurveys.Count} staff surveys submitted, {avgSatisfaction:F1}% average satisfaction | Form: Data Entry → Staff Surveys"
                : "No staff surveys yet - Go to Data Entry → Staff Surveys", nextId++);

        // 2. Professional Development (employee participation + activity totals)
        var profDevs = await _context.ProfessionalDevelopments.ToListAsync();
        var participatingEmployees = profDevs.Count;
        
        AddOrUpdateMetric(goal, "Professional Development Plans", "Staff growth initiatives", 
            participatingEmployees, "employees", "25", profDevs.Any() ? "Active" : "Planning",
            profDevs.Any()
                ? $"{participatingEmployees} employees participating | Form: Data Entry → Professional Development"
                : "No professional development yet - Go to Data Entry → Professional Development", nextId++);

        // 3. Board Member Recruitment
        var boardMembers = await _context.BoardMember_29D.ToListAsync();
        var totalRecruited = boardMembers.Sum(b => b.NumberRecruited ?? 0);
        
        AddOrUpdateMetric(goal, "Board Recruitment", "New board member acquisition", 
            totalRecruited, "members", "10", boardMembers.Any() ? "Active" : "Planning",
            boardMembers.Any() ? $"Board members recruited: {totalRecruited}" : "No board recruitment data yet - Go to Data Entry → Board Management", nextId++);

        // 4. Board Meeting Attendance
        var boardAttendance = await _context.BoardMeetingAttendance.ToListAsync();
        decimal avgAttendanceRate = 0;
        if (boardAttendance.Any())
        {
            var attendanceRates = boardAttendance
                .Where(b => b.TotalBoardMembers > 0)
                .Select(b => (double)b.MembersInAttendance / b.TotalBoardMembers * 100);
            var average = attendanceRates.Any() ? attendanceRates.Average() : 0;
            avgAttendanceRate = average.HasValue ? (decimal)average.Value : 0;
        }
        
        AddOrUpdateMetric(goal, "Board Meeting Participation", "Average meeting attendance", 
            Math.Round(avgAttendanceRate, 1), "%", "90", boardAttendance.Any() ? "Active" : "Planning",
            boardAttendance.Any() ? $"Average attendance rate: {avgAttendanceRate:F1}%" : "No board attendance data yet - Go to Data Entry → Board Management", nextId++);

        // 5. Board Self-Assessment
        var boardSelfAssessments = await _context.selfAssess_31D.ToListAsync();
        var avgBoardSelfAssessment = boardSelfAssessments.Any()
            ? boardSelfAssessments.Average(a => a.SelfAssessmentScore)
            : 0;

        AddOrUpdateMetric(goal, "Board Self-Assessment", "Average board annual self-assessment score",
            Math.Round((decimal)avgBoardSelfAssessment, 1), "%", "85", boardSelfAssessments.Any() ? "Active" : "Planning",
            boardSelfAssessments.Any()
                ? $"{boardSelfAssessments.Count} entries, {avgBoardSelfAssessment:F1}% average score | Form: Data Entry → Board Self-Assessment"
                : "No board self-assessment data yet - Go to Data Entry → Board Self-Assessment", nextId++);

        // 6. Volunteer Program
        var volunteerPrograms = await _context.volunteerProgram_40D.ToListAsync();
        var totalVolunteers = volunteerPrograms.Sum(v => v.NumberOfVolunteers);
        var totalVolunteerInitiatives = volunteerPrograms.Sum(v => v.VolunteerLedInitiatives);

        AddOrUpdateMetric(goal, "Volunteer Program Participation", "Total volunteers and volunteer-led initiatives",
            totalVolunteers, "volunteers", "100", volunteerPrograms.Any() ? "Active" : "Planning",
            volunteerPrograms.Any()
                ? $"{totalVolunteers} volunteers across {volunteerPrograms.Count} entries, {totalVolunteerInitiatives} volunteer-led initiatives | Form: Data Entry → Volunteer Program"
                : "No volunteer program data yet - Go to Data Entry → Volunteer Program", nextId++);

        var volunteerMetric = goal.Metrics.FirstOrDefault(m => m.Name == "Volunteer Program Participation");
        if (volunteerMetric != null)
        {
            volunteerMetric.Q1Value = totalVolunteerInitiatives;
            volunteerMetric.Q2Value = volunteerPrograms.Count;
        }
    }

    private async Task AddFinancialMetricsAsync(StrategicGoal goal, string fiscalYear)
    {
        var nextId = goal.Metrics.Count + 3000;
        var hasFiscalYear = TryParseFiscalYearEnd(fiscalYear, out var fiscalYearEnd);
        var fiscalYearStart = fiscalYearEnd - 1;

        // 1. Budget Tracking
        var budgetTracking = await _context.BudgetTracking_28D.ToListAsync();
        if (hasFiscalYear)
        {
            var fiscalYearBudgetTracking = budgetTracking
                .Where(b => b.Year == fiscalYearStart || b.Year == fiscalYearEnd)
                .ToList();
            budgetTracking = fiscalYearBudgetTracking.Any() ? fiscalYearBudgetTracking : budgetTracking;
        }
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

        // 2. Fee-for-Service Revenue
        var feeServices = await _context.FeeForServices_21D.ToListAsync();
        if (hasFiscalYear)
        {
            var fiscalYearFeeServices = feeServices
                .Where(f => f.Year == fiscalYearStart || f.Year == fiscalYearEnd)
                .ToList();
            feeServices = fiscalYearFeeServices.Any() ? fiscalYearFeeServices : feeServices;
        }
        var totalFeeRevenue = feeServices.Sum(f => f.RevenueReceived);
        var totalFeeExpenses = feeServices.Sum(f => f.ExpenseReceived);
        var totalFeeNetRevenue = totalFeeRevenue - totalFeeExpenses;
        var totalFeeServices = feeServices.Count;
        var totalFeeAttendees = feeServices.Sum(f => f.NumberOfAttendees);
        var avgRevenuePerService = totalFeeServices > 0 ? totalFeeNetRevenue / totalFeeServices : 0;
        
        AddOrUpdateMetric(goal, "Fee-for-Service Income", "Service-based revenue generation", 
            totalFeeNetRevenue, "dollars", "75000", feeServices.Any() ? "Active" : "Planning",
            feeServices.Any() ? $"Gross: ${totalFeeRevenue:N0}, Expenses: ${totalFeeExpenses:N0}, Net: ${totalFeeNetRevenue:N0} from {totalFeeServices} services, {totalFeeAttendees} attendees" : "No fee-for-service data yet - Go to Data Entry → Fee for Services", nextId++);

        // Store supporting values for dashboard card display
        var feeMetric = goal.Metrics.FirstOrDefault(m => m.Name == "Fee-for-Service Income");
        if (feeMetric != null)
        {
            feeMetric.Q1Value = totalFeeServices;
            feeMetric.Q2Value = totalFeeAttendees;
            feeMetric.Q3Value = Math.Round(avgRevenuePerService, 2);
        }

        // 3. Income Tracking
        var incomeData = await _context.income_27D.ToListAsync();
        if (hasFiscalYear)
        {
            var fiscalYearIncomeData = incomeData
                .Where(i =>
                {
                    var effectiveYear = GetIncomeRecordYear(i);
                    return !effectiveYear.HasValue || effectiveYear.Value == fiscalYearStart || effectiveYear.Value == fiscalYearEnd;
                })
                .ToList();
            incomeData = fiscalYearIncomeData.Any() ? fiscalYearIncomeData : incomeData;
        }
        var totalIncome = incomeData.Sum(i => i.Amount);
        
        AddOrUpdateMetric(goal, "General Income Streams", "Diversified income tracking", 
            totalIncome, "dollars", "100000", incomeData.Any() ? "Active" : "Planning",
            incomeData.Any() ? $"Total income: ${totalIncome:N0} from {incomeData.Count} sources" : "No income data yet - Go to Data Entry → Income Tracking", nextId++);

        var incomeMetric = goal.Metrics.FirstOrDefault(m => m.Name == "General Income Streams");
        if (incomeMetric != null)
        {
            incomeMetric.Q1Value = incomeData.Count;
        }

        // 4. Donor Events
        var donorEvents = await _context.DonorEvents_19D.ToListAsync();
        var totalParticipants = donorEvents.Sum(d => d.NumberOfParticipants);
        var avgSatisfaction = donorEvents.Any() ? donorEvents.Average(d => d.EventSatisfactionRating) : 0;
        
        AddOrUpdateMetric(goal, "Donor Engagement Events", "Fundraising event effectiveness", 
            totalParticipants, "participants", "200", donorEvents.Any() ? "Active" : "Planning",
            donorEvents.Any() ? $"{donorEvents.Count} events, {totalParticipants} participants, {avgSatisfaction:F1}% avg satisfaction" : "No donor events yet - Go to Data Entry → Donor Events", nextId++);

        // Store donor event count for dashboard card visualizations
        var donorEngagementMetric = goal.Metrics.FirstOrDefault(m => m.Name == "Donor Engagement Events");
        if (donorEngagementMetric != null)
        {
            donorEngagementMetric.Q1Value = donorEvents.Count;
            donorEngagementMetric.Q2Value = Math.Round((decimal)avgSatisfaction, 1);
        }

        // 5. Donor Communication Satisfaction (from Communication Rate form)
        var commRate = await _context.CommunicationRate.ToListAsync();
        var avgCommSatisfaction = commRate.Any() ? commRate.Average(c => c.AverageCommunicationSatisfaction) : 0;

        AddOrUpdateMetric(goal, "Donor Communication Satisfaction", "Annual donor satisfaction with communications",
            Math.Round((decimal)avgCommSatisfaction, 1), "%", "85", commRate.Any() ? "Active" : "Planning",
            commRate.Any()
                ? $"{commRate.Count} communication entries, {avgCommSatisfaction:F1}% average satisfaction | Form: Data Entry → Communication Rate"
                : "No communication rate data yet - Go to Data Entry → Communication Rate", nextId++);

        // Ensure core financial cards always have backing metrics.
        EnsureFinancialFallbackMetrics(goal);
    }

    private async Task AddCommunityMetricsAsync(StrategicGoal goal)
    {
        var nextId = goal.Metrics.Count + 4000;

        // 1. Communication Rate
        var commRate = await _context.CommunicationRate.ToListAsync();
        var avgCommSatisfaction = commRate.Any() ? commRate.Average(c => c.AverageCommunicationSatisfaction) : 0;
        
        AddOrUpdateMetric(goal, "Community Communications", "Outreach and engagement satisfaction", 
            avgCommSatisfaction, "%", "85", commRate.Any() ? "Active" : "Planning",
            $"📢 {commRate.Count} communication entries, {avgCommSatisfaction:F1}% satisfaction | Form: Data Entry → Communications", nextId++);

        // 5. Event Satisfaction (from EventSatisfaction_12D)
        var eventSatisfaction = await _context.EventSatisfaction_12D.ToListAsync();
        var avgEventSatisfaction = eventSatisfaction.Any() ? eventSatisfaction.Average(e => e.EventAttendeeSatisfactionPercentage) : 0;
        
        AddOrUpdateMetric(goal, "Event Quality Score", "Community event satisfaction", 
            avgEventSatisfaction, "%", "90", eventSatisfaction.Any() ? "Active" : "Planning",
            $"🎉 Event Satisfaction: {avgEventSatisfaction:F1}% from {eventSatisfaction.Count} events | Form: Data Entry → Event Satisfaction", nextId++);

        // 6. Youth Program Satisfaction (Update existing metric - general event satisfaction as proxy)
        var avgYouthSatisfaction = eventSatisfaction.Any() ? eventSatisfaction.Average(e => e.EventAttendeeSatisfactionPercentage) : 0;
        
        AddOrUpdateMetric(goal, "Youth Program Satisfaction", "Average satisfaction across all youth programs", 
            avgYouthSatisfaction, "%", "85", eventSatisfaction.Any() ? "Active" : "Planning",
            $"👥 {avgYouthSatisfaction:F1}% satisfaction from community events (youth-specific data via Event Satisfaction form) | Form: Data Entry → Event Satisfaction", nextId++);

        // Note: Community Trust Rating moved to Identity/Value Proposition tab
        // as "Community Perception Survey" to better match the actual form
    }

    private void AddOrUpdateMetric(StrategicGoal goal, string name, string description, decimal currentValue, 
                                 string unit, string target, string status, string detailedDescription, int id)
    {
        var existingMetric = goal.Metrics.FirstOrDefault(m => m.Name == name);
        var hasIncomingData = !string.Equals(status, "Planning", StringComparison.OrdinalIgnoreCase);
        
        if (existingMetric != null)
        {
            // Keep last known real value when no new data arrived this cycle.
            var effectiveCurrentValue = hasIncomingData ? currentValue : existingMetric.CurrentValue;

            existingMetric.CurrentValue = effectiveCurrentValue;
            existingMetric.Status = ResolveMetricStatus(existingMetric.Status, effectiveCurrentValue, target, hasIncomingData);
            if (hasIncomingData)
            {
                existingMetric.Description = detailedDescription;
            }
            existingMetric.Target = target;
            existingMetric.Unit = unit;
        }
        else
        {
            var resolvedStatus = ResolveMetricStatus(status, currentValue, target, hasIncomingData);

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
                Status = resolvedStatus,
                TargetDate = DateTime.Now.AddMonths(12)
            });
        }
    }

    private string ResolveMetricStatus(string priorStatus, decimal currentValue, string target, bool hasIncomingData)
    {
        if (TryParseMetricTarget(target, out var targetValue) && targetValue > 0 && currentValue >= targetValue)
        {
            return "Completed";
        }

        if (currentValue > 0
            || hasIncomingData
            || string.Equals(priorStatus, "Active", StringComparison.OrdinalIgnoreCase)
            || string.Equals(priorStatus, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            return "Active";
        }

        return "Planning";
    }

    private bool TryParseMetricTarget(string target, out decimal targetValue)
    {
        targetValue = 0;
        if (string.IsNullOrWhiteSpace(target))
        {
            return false;
        }

        var cleaned = new string(target.Where(c => char.IsDigit(c) || c == '.' || c == '-').ToArray());
        if (string.IsNullOrWhiteSpace(cleaned))
        {
            return false;
        }

        return decimal.TryParse(cleaned, NumberStyles.Number, CultureInfo.InvariantCulture, out targetValue);
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

    private int? ResolveGoalIdForEvent(Event evt)
    {
        if (!evt.StrategyId.HasValue)
        {
            return null;
        }

        return _context.Strategies
            .Where(s => s.Id == evt.StrategyId.Value)
            .Select(s => (int?)s.StrategicGoalId)
            .FirstOrDefault();
    }

    private string GetGoalNameById(int goalId)
    {
        return goalId switch
        {
            1 => "Organizational Building",
            2 => "Financial Sustainability",
            3 => "Identity/Value Proposition",
            4 => "Community Engagement",
            _ => "Strategic Goal"
        };
    }

    private string GetColorByGoalId(int goalId)
    {
        return goalId switch
        {
            1 => "var(--onejax-navy)",
            2 => "var(--onejax-green)",
            3 => "var(--onejax-orange)",
            4 => "var(--onejax-blue)",
            _ => "var(--onejax-navy)"
        };
    }
}
