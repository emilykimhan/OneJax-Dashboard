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
            var hasFiscalYearParam = Request.Query.ContainsKey("fiscalYear");
            var defaultFiscalYear = GetCurrentFiscalYearLabel();
            var selectedFiscalYear = hasFiscalYearParam
                ? (fiscalYear ?? string.Empty).Trim()
                : defaultFiscalYear;

            ViewBag.SelectedFiscalYear = selectedFiscalYear;
            ViewBag.ActiveFiscalYearLabel = string.IsNullOrWhiteSpace(selectedFiscalYear) ? "All Years" : selectedFiscalYear;
            ViewBag.FiscalYearOptions = GetFiscalYearOptions(defaultFiscalYear, yearsBefore: 2, yearsAfter: 2);

            // Create enhanced dashboard data with comprehensive metrics
            var dashboardData = await BuildEnhancedDashboardAsync(selectedFiscalYear);
            var overallGoals = await CreateAllStrategicGoalsAsync();
            await EnhanceGoalsWithComprehensiveMetricsAsync(overallGoals, string.Empty);
            PopulateDashboardComputedValues(dashboardData, overallGoals);
            dashboardData.StrategicGoals = overallGoals;

            // Apply filters
            dashboardData = ApplyFilters(dashboardData, status ?? "", time ?? "", goal ?? "", selectedFiscalYear, quarter ?? "");

            // Add board meeting attendance records for the enhanced view
            try
            {
                var boardMeetingAttendance = FilterByFiscalYearDate(
                        await _context.BoardMeetingAttendance
                            .OrderByDescending(b => b.MeetingDate)
                            .ToListAsync(),
                        selectedFiscalYear,
                        b => b.MeetingDate)
                    .OrderByDescending(b => b.MeetingDate)
                    .ToList();

                ViewBag.BoardMeetingAttendance = boardMeetingAttendance.Take(10).ToList();
                ViewBag.BoardMeetingAttendanceCount = boardMeetingAttendance.Count;
                ViewBag.BoardMeetingAttendanceAverage = boardMeetingAttendance
                    .Where(b => (b.TotalBoardMembers ?? 0) > 0)
                    .Select(b => Math.Round((decimal)b.MembersInAttendance / (b.TotalBoardMembers ?? 1) * 100m, 1))
                    .DefaultIfEmpty(0m)
                    .Average();
            }
            catch
            {
                ViewBag.BoardMeetingAttendance = new List<BoardMeetingAttendance>();
                ViewBag.BoardMeetingAttendanceCount = 0;
                ViewBag.BoardMeetingAttendanceAverage = 0m;
            }

            // Add budget tracking data for Financial Sustainability chart
            try
            {
                var budgetTracking = await _context.BudgetTracking_28D.ToListAsync();
                budgetTracking = FilterByFiscalYearYears(budgetTracking, selectedFiscalYear, b => b.Year);
                ViewBag.BudgetTracking = budgetTracking;
            }
            catch
            {
                ViewBag.BudgetTracking = new List<BudgetTracking_28D>();
            }

            try
            {
                var staffSurveyRecords = FilterByFiscalYearYearMonthWithCreatedDateFallback(
                        await _context.StaffSurveys_22D
                            .OrderByDescending(s => s.Year)
                            .ThenByDescending(s => s.CreatedDate)
                            .ToListAsync(),
                        selectedFiscalYear,
                        s => s.Year,
                        s => s.Month,
                        s => s.CreatedDate)
                    .OrderByDescending(s => s.Year)
                    .ThenByDescending(s => s.CreatedDate)
                    .ToList();

                ViewBag.StaffSurveyRecords = staffSurveyRecords;
                ViewBag.StaffSurveyAverage = staffSurveyRecords.Any()
                    ? Math.Round((decimal)staffSurveyRecords.Average(s => s.SatisfactionRate), 1)
                    : 0m;
            }
            catch
            {
                ViewBag.StaffSurveyRecords = new List<StaffSurvey_22D>();
                ViewBag.StaffSurveyAverage = 0m;
            }

            try
            {
                var boardSelfAssessmentRecords = FilterByFiscalYearYearMonthWithCreatedDateFallback(
                        await _context.selfAssess_31D
                            .OrderByDescending(a => a.Year)
                            .ThenByDescending(a => a.CreatedDate)
                            .ToListAsync(),
                        selectedFiscalYear,
                        a => a.Year,
                        a => a.Month,
                        a => a.CreatedDate)
                    .OrderByDescending(a => a.Year)
                    .ThenByDescending(a => a.CreatedDate)
                    .ToList();

                ViewBag.BoardSelfAssessmentRecords = boardSelfAssessmentRecords;
                ViewBag.BoardSelfAssessmentAverage = boardSelfAssessmentRecords.Any()
                    ? Math.Round((decimal)boardSelfAssessmentRecords.Average(a => a.SelfAssessmentScore), 1)
                    : 0m;
            }
            catch
            {
                ViewBag.BoardSelfAssessmentRecords = new List<selfAssess_31D>();
                ViewBag.BoardSelfAssessmentAverage = 0m;
            }

            try
            {
                var professionalDevelopmentRecords = FilterByFiscalYearYearMonthWithCreatedDateFallback(
                        await _context.ProfessionalDevelopments
                            .OrderByDescending(p => p.Year)
                            .ThenByDescending(p => p.CreatedDate)
                            .ToListAsync(),
                        selectedFiscalYear,
                        p => p.Year,
                        p => p.Month,
                        p => p.CreatedDate)
                    .OrderByDescending(p => p.Year)
                    .ThenByDescending(p => p.CreatedDate)
                    .ToList();

                ViewBag.ProfessionalDevelopmentRecords = professionalDevelopmentRecords;
                ViewBag.ProfessionalDevelopmentParticipantCount = professionalDevelopmentRecords
                    .Select(p => p.Name)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count();
                ViewBag.ProfessionalDevelopmentAccountCount = await _context.Staffauth.CountAsync();
            }
            catch
            {
                ViewBag.ProfessionalDevelopmentRecords = new List<ProfessionalDevelopment>();
                ViewBag.ProfessionalDevelopmentParticipantCount = 0;
                ViewBag.ProfessionalDevelopmentAccountCount = 0;
            }

            try
            {
                var boardRecruitmentRecords = FilterByFiscalYearCalendarQuarterWithCreatedDateFallback(
                        await _context.BoardMember_29D
                            .OrderByDescending(b => b.Year)
                            .ThenByDescending(b => b.Quarter)
                            .ThenByDescending(b => b.CreatedDate)
                            .ToListAsync(),
                        selectedFiscalYear,
                        b => b.Year,
                        b => b.Quarter,
                        b => b.CreatedDate)
                    .OrderByDescending(b => b.Year)
                    .ThenByDescending(b => b.Quarter)
                    .ThenByDescending(b => b.CreatedDate)
                    .ToList();

                ViewBag.BoardRecruitmentRecords = boardRecruitmentRecords;
            }
            catch
            {
                ViewBag.BoardRecruitmentRecords = new List<BoardMemberRecruitment>();
            }

            // Add volunteer program records for detailed organizational card display
            try
            {
                var volunteerProgramRecords = await _context.volunteerProgram_40D
                    .OrderByDescending(v => v.Year)
                    .ThenByDescending(v => v.Quarter)
                    .ThenByDescending(v => v.CreatedDate)
                    .ToListAsync();

                var filteredVolunteerProgramRecords = FilterByFiscalYearCalendarQuarterWithCreatedDateFallback(
                        volunteerProgramRecords,
                        selectedFiscalYear,
                        v => v.Year,
                        v => v.Quarter,
                        v => v.CreatedDate)
                    .OrderByDescending(v => v.Year)
                    .ThenByDescending(v => v.Quarter)
                    .ThenByDescending(v => v.CreatedDate)
                    .ToList();

                ViewBag.VolunteerProgramRecords = filteredVolunteerProgramRecords.Take(12).ToList();
                ViewBag.VolunteerProgramAllFilteredRecords = filteredVolunteerProgramRecords;

                var volunteerTargetYearCount = 1;
                if (!TryParseFiscalYearEnd(selectedFiscalYear, out _)
                    && filteredVolunteerProgramRecords.Any())
                {
                    var volunteerFiscalYearEnds = filteredVolunteerProgramRecords
                        .Select(v => GetFiscalYearEndFromCalendarQuarterRecord(v.Year, v.Quarter)
                            ?? GetFiscalYearEndFromDate(v.CreatedDate))
                        .ToList();

                    if (volunteerFiscalYearEnds.Any())
                    {
                        volunteerTargetYearCount = Math.Max(1, volunteerFiscalYearEnds.Max() - volunteerFiscalYearEnds.Min() + 1);
                    }
                }

                ViewBag.VolunteerProgramTargetYearCount = volunteerTargetYearCount;
            }
            catch
            {
                ViewBag.VolunteerProgramRecords = new List<volunteerProgram_40D>();
                ViewBag.VolunteerProgramAllFilteredRecords = new List<volunteerProgram_40D>();
                ViewBag.VolunteerProgramTargetYearCount = 1;
            }

            return View(dashboardData);
        }
        catch (Exception ex)
        {
            // Enhanced error logging for Azure diagnostics
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<HomeController>>();
            logger.LogError(ex, "Dashboard data loading failed. FiscalYear: {FiscalYear}, Goal: {Goal}", fiscalYear, goal);
            
            // Show error message but keep the full tab structure so the UI doesn't look "gone".
            // This also makes it easier to keep working while a specific table/query is broken.
            List<StrategicGoal> fallbackGoals;
            try
            {
                fallbackGoals = await CreateAllStrategicGoalsAsync();
            }
            catch
            {
                fallbackGoals = new List<StrategicGoal>
                {
                    new() { Id = 1, Name = "Organizational Building", Description = "", Color = "var(--onejax-navy)", Events = new List<Event>(), Metrics = new List<GoalMetric>() },
                    new() { Id = 2, Name = "Financial Sustainability", Description = "", Color = "var(--onejax-green)", Events = new List<Event>(), Metrics = new List<GoalMetric>() },
                    new() { Id = 3, Name = "Identity/Value Proposition", Description = "", Color = "var(--onejax-orange)", Events = new List<Event>(), Metrics = new List<GoalMetric>() },
                    new() { Id = 4, Name = "Community Engagement", Description = "", Color = "var(--onejax-blue)", Events = new List<Event>(), Metrics = new List<GoalMetric>() }
                };
            }

            var errorData = new DashboardViewModel
            {
                StrategicGoals = fallbackGoals,
                HasError = true,
                ErrorMessage = ex.ToString(),
                DataSource = "Dashboard Load Error (Fallback Tabs Rendered)"
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

    private static string GetCurrentFiscalYearLabel(DateTime? currentDate = null)
    {
        var today = currentDate ?? DateTime.Today;
        var fiscalYearEnd = today.Month >= 7 ? today.Year + 1 : today.Year;
        return $"{fiscalYearEnd - 1}-{fiscalYearEnd}";
    }

    private static List<string> GetFiscalYearOptions(string centerFiscalYear, int yearsBefore, int yearsAfter)
    {
        if (!TryParseFiscalYearEndStatic(centerFiscalYear, out var centerFiscalYearEnd))
        {
            centerFiscalYearEnd = DateTime.Today.Month >= 7 ? DateTime.Today.Year + 1 : DateTime.Today.Year;
        }

        return Enumerable.Range(centerFiscalYearEnd - yearsBefore, yearsBefore + yearsAfter + 1)
            .Select(yearEnd => $"{yearEnd - 1}-{yearEnd}")
            .OrderByDescending(label => label)
            .ToList();
    }

    private static bool TryParseFiscalYearEndStatic(string fiscalYear, out int fiscalYearEnd)
    {
        fiscalYearEnd = 0;
        if (string.IsNullOrWhiteSpace(fiscalYear))
        {
            return false;
        }

        var parts = fiscalYear.Split(new[] { '-', '/' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 2
            && int.TryParse(parts[0], out var startYear)
            && int.TryParse(parts[1], out var endYear)
            && endYear == startYear + 1)
        {
            fiscalYearEnd = endYear;
            return true;
        }

        return int.TryParse(fiscalYear, out fiscalYearEnd);
    }

    private static int GetFiscalYearEndFromDate(DateTime date)
    {
        return date.Month >= 7 ? date.Year + 1 : date.Year;
    }

    private static int? GetFiscalYearEndFromStoredYear(int year)
    {
        return year > 0 ? year : null;
    }

    private static int? TryParseMonthNumber(string? month)
    {
        if (string.IsNullOrWhiteSpace(month))
        {
            return null;
        }

        if (DateTime.TryParseExact(
                month.Trim(),
                new[] { "MMMM", "MMM" },
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var parsedDate))
        {
            return parsedDate.Month;
        }

        return null;
    }

    private static int? GetFiscalYearEndFromStoredYearMonth(int year, string? month)
    {
        if (year <= 0)
        {
            return null;
        }

        var monthNumber = TryParseMonthNumber(month);
        if (!monthNumber.HasValue)
        {
            return GetFiscalYearEndFromStoredYear(year);
        }

        return GetFiscalYearEndFromDate(new DateTime(year, monthNumber.Value, 1));
    }

    private static int? GetFiscalYearEndFromCalendarQuarterRecord(int year, int quarter)
    {
        if (year <= 0 || quarter is < 1 or > 4)
        {
            return null;
        }

        return quarter switch
        {
            1 or 2 => year,
            3 or 4 => year + 1,
            _ => null
        };
    }

    private List<T> FilterByResolvedFiscalYear<T>(IEnumerable<T> source, string fiscalYear, Func<T, int?> fiscalYearEndSelector)
    {
        if (!TryParseFiscalYearEnd(fiscalYear, out var selectedFiscalYearEnd))
        {
            return source.ToList();
        }

        return source
            .Where(item => fiscalYearEndSelector(item) == selectedFiscalYearEnd)
            .ToList();
    }

    private List<T> FilterByFiscalYearDate<T>(IEnumerable<T> source, string fiscalYear, Func<T, DateTime> dateSelector)
    {
        return FilterByResolvedFiscalYear(source, fiscalYear, item => GetFiscalYearEndFromDate(dateSelector(item)));
    }

    private List<T> FilterByFiscalYearYears<T>(IEnumerable<T> source, string fiscalYear, Func<T, int> yearSelector)
    {
        return FilterByResolvedFiscalYear(source, fiscalYear, item => GetFiscalYearEndFromStoredYear(yearSelector(item)));
    }

    private List<T> FilterByFiscalYearYearMonthWithCreatedDateFallback<T>(
        IEnumerable<T> source,
        string fiscalYear,
        Func<T, int> yearSelector,
        Func<T, string?> monthSelector,
        Func<T, DateTime> createdDateSelector)
    {
        return FilterByResolvedFiscalYear(
            source,
            fiscalYear,
            item => GetFiscalYearEndFromStoredYearMonth(yearSelector(item), monthSelector(item))
                ?? GetFiscalYearEndFromDate(createdDateSelector(item)));
    }

    private List<T> FilterByFiscalYearCalendarQuarterWithCreatedDateFallback<T>(
        IEnumerable<T> source,
        string fiscalYear,
        Func<T, int> yearSelector,
        Func<T, int> quarterSelector,
        Func<T, DateTime> createdDateSelector)
    {
        return FilterByResolvedFiscalYear(
            source,
            fiscalYear,
            item => GetFiscalYearEndFromCalendarQuarterRecord(yearSelector(item), quarterSelector(item))
                ?? GetFiscalYearEndFromDate(createdDateSelector(item)));
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

    private void PopulateDashboardComputedValues(DashboardViewModel dashboard, IEnumerable<StrategicGoal> overallGoals)
    {
        var goals = overallGoals?.ToList() ?? new List<StrategicGoal>();
        var allMetrics = goals
            .SelectMany(goal => goal.Metrics ?? new List<GoalMetric>())
            .Where(DashboardMetricRules.CountsTowardOverallProgress)
            .ToList();

        foreach (var goal in goals)
        {
            var goalMetrics = (goal.Metrics ?? new List<GoalMetric>())
                .Where(DashboardMetricRules.CountsTowardOverallProgress)
                .ToList();

            goal.OverallScheduledMetricCount = goalMetrics.Count;
            goal.OverallReportingMetricCount = goalMetrics.Count(m => DashboardMetricRules.IsReportingMetric(m, string.Empty));
            goal.OverallProgress = DashboardMetricRules.CalculateGoalProgress(goalMetrics, string.Empty);
            goal.OverallEventCount = goal.Events?.Count ?? 0;

            goal.ScheduledMetricCount = goal.OverallScheduledMetricCount;
            goal.ReportingMetricCount = goal.OverallReportingMetricCount;
            goal.Progress = goal.OverallProgress;
        }

        dashboard.InScopeMetricsCount = allMetrics.Count;
        dashboard.ReportingMetricsCount = allMetrics.Count(m => DashboardMetricRules.IsReportingMetric(m, string.Empty));
        dashboard.ActiveMetricsCount = dashboard.ReportingMetricsCount;
        dashboard.MetricsAtTargetCount = allMetrics.Count(DashboardMetricRules.IsMetricAtTarget);
        dashboard.TotalEligibleMetrics = dashboard.InScopeMetricsCount;
        dashboard.TotalMetricsMeetingGoal = dashboard.MetricsAtTargetCount;
        dashboard.OverallDashboardProgress = goals.Any()
            ? Math.Round(goals.Average(g => g.OverallProgress), 1)
            : 0m;
        dashboard.PublicEventsCount = goals.Sum(g => g.Events?.Count ?? 0);
        dashboard.ReportingMetricsPercentage = dashboard.InScopeMetricsCount > 0
            ? Math.Round((double)dashboard.ReportingMetricsCount / dashboard.InScopeMetricsCount * 100, 1)
            : 0;
        dashboard.ActiveMetricsPercentage = dashboard.InScopeMetricsCount > 0
            ? Math.Round((double)dashboard.ActiveMetricsCount / dashboard.InScopeMetricsCount * 100, 1)
            : 0;
        dashboard.MetricsAtTargetPercentage = dashboard.InScopeMetricsCount > 0
            ? Math.Round((double)dashboard.MetricsAtTargetCount / dashboard.InScopeMetricsCount * 100, 1)
            : 0;
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
        var hasStrategyEvents = _context.Strategies.Any();
        
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
        
        // Get strategies for this strategic goal
        var strategiesForGoal = _context.Strategies
            .Where(s => s.StrategicGoalId == strategicGoalId)
            .Select(s => s.Id)
            .ToList();
        
        // Get database events linked to those strategies
        // Events are linked to Strategies, not directly to StrategicGoals
        var dbEvents = _context.Events
            .Where(e => e.StrategyId.HasValue && strategiesForGoal.Contains(e.StrategyId.Value))
            .ToList();
        
        events.AddRange(dbEvents);
        
        // Get events from database Strategies table (Core Strategies events).
        // IMPORTANT: The local SQLite schema may lag behind the current EF model (e.g. missing ArchivedAtUtc/ProgramType),
        // so do a minimal raw SQL read of the columns that exist in older DBs.
        try
        {
            var dbStrategies = LoadStrategiesForGoal(strategicGoalId);

            foreach (var strategy in dbStrategies)
            {
                // Only add if not already in Events table
                if (!dbEvents.Any(e => e.Title == strategy.Name && e.Description == strategy.Description))
                {
                    var typeLabel =
                        !string.IsNullOrWhiteSpace(strategy.ProgramName)
                            ? strategy.ProgramName
                            : (!string.IsNullOrWhiteSpace(strategy.EventType) ? strategy.EventType : "Program");

                    events.Add(new Event
                    {
                        Id = strategy.Id + 1000, // Offset to avoid ID conflicts
                        Title = strategy.Name ?? "",
                        Description = strategy.Description ?? "",
                        Type = typeLabel,
                        Status = "Planned",
                        StrategicGoalId = strategicGoalId, // This is NotMapped, safe to set for display
                        DueDate = DateTime.TryParse(strategy.Date, out var date) ? date : DateTime.Now.AddDays(30),
                        Notes = $"Core Strategy Event. {(!string.IsNullOrEmpty(strategy.Time) ? $"Time: {strategy.Time}" : "")} {(!string.IsNullOrEmpty(strategy.EventFYear) ? $"Fiscal Year: {strategy.EventFYear}" : "")}",
                        Attendees = 0,
                        Location = "TBD"
                    });
                }
            }
        }
        catch
        {
            // If the Strategies table/schema is not compatible, keep going with Events + static list.
        }
        
        // Also check the static list for backward compatibility
        // Note: StrategyController doesn't have a static Strategies property, all strategies are in the database
        // var staticStrategies = StrategyController.Strategies.Where(s => s.StrategicGoalId == strategicGoalId).ToList();
        
        // Since static strategies are no longer used, we'll just return the events from database
        
        return events.OrderBy(e => e.DueDate).ToList();
    }

    private sealed class StrategyRow
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ProgramName { get; set; }
        public string? Date { get; set; }
        public string? Time { get; set; }
        public string? EventFYear { get; set; }
        public string? EventType { get; set; }
    }

    private List<StrategyRow> LoadStrategiesForGoal(int strategicGoalId)
    {
        var results = new List<StrategyRow>();

        using var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            connection.Open();
        }

        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"
SELECT Id, Name, Description, ProgramName, Date, Time, EventFYear, EventType
FROM Strategies
WHERE StrategicGoalId = $goalId
";
        var p = cmd.CreateParameter();
        p.ParameterName = "$goalId";
        p.Value = strategicGoalId;
        cmd.Parameters.Add(p);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            results.Add(new StrategyRow
            {
                Id = reader.GetInt32(0),
                Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                ProgramName = reader.IsDBNull(3) ? null : reader.GetString(3),
                Date = reader.IsDBNull(4) ? null : reader.GetString(4),
                Time = reader.IsDBNull(5) ? null : reader.GetString(5),
                EventFYear = reader.IsDBNull(6) ? null : reader.GetString(6),
                EventType = reader.IsDBNull(7) ? null : reader.GetString(7),
            });
        }

        return results;
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

            // Events (from all sources: Events table, Strategies table, and static list)
            try
            {
                var totalEvents = 0;
                
                // Count from Events table
                totalEvents += _context.Events.Count();
                
                // Count from Strategies table (Core Strategies events)
                totalEvents += _context.Strategies.Count();
                
                // Note: Static events from StrategyController no longer exist - all events are in database
                
                summary.TotalEvents = totalEvents;
            }
            catch
            {
                // Fallback if database access fails
                summary.TotalEvents = 0; // Since static strategies no longer exist
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
                    activities.Add(new RecentActivity
                    {
                        Type = "Event",
                        Title = evt.Title,
                        Description = $"{evt.Type} | Status: {evt.Status} | {(evt.StartDate?.ToString("MMM dd") ?? evt.DueDate?.ToString("MMM dd") ?? "Date TBD")}",
                        Date = evt.StartDate ?? evt.DueDate ?? DateTime.Now,
                        Icon = "fas fa-calendar-check",
                        Color = GetColorByGoalId(evt.StrategicGoalId ?? 1),
                        GoalName = GetGoalNameById(evt.StrategicGoalId ?? 1)
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

        // Note: ProfessionalDevelopmentCount field was removed from StaffSurvey_22D in migrations
        // This metric is no longer tracked via staff surveys but via dedicated professional development records
        goal.Metrics.Add(new GoalMetric
        {
            Id = 2,
            Name = "Professional Development Activities (Individual Records)",
            Description = profDev.Any() ? $"Activities tracked via dedicated PD records" : "No individual PD records available - use dedicated PD form",
            StrategicGoalId = 1,
            Target = "",
            CurrentValue = profDev.Count,
            Unit = "records",
            Status = profDev.Any() ? "In Progress" : "Pending Data",
            TargetDate = DateTime.Now.AddMonths(12)
        });

        // Note: ProfessionalDevelopmentYear26 and ProfessionalDevelopmentYear27 fields were removed from ProfessionalDevelopment in migrations
        // Now we count total professional development records/activities instead
        var totalActivitiesTracked = profDev.Count; // Number of PD records submitted
        var totalActivitiesDescription = profDev.Any() 
            ? string.Join(", ", profDev.Take(3).Select(p => $"{p.Name}: {p.Activities.Split(',').Length} activities"))
            : "No activities tracked yet";
        
        goal.Metrics.Add(new GoalMetric
        {
            Id = 3,
            Name = "Professional Development Planning",
            Description = profDev.Any() ? $"Total PD records: {totalActivitiesTracked}. Examples: {totalActivitiesDescription}" : "No planning data available - showing baseline",
            StrategicGoalId = 1,
            Target = "",
            CurrentValue = totalActivitiesTracked,
            Unit = "records",
            Status = profDev.Any() ? "In Progress" : "Pending Data",
            TargetDate = DateTime.Now.AddMonths(12),
            Q1Value = profDev.Count(p => p.CreatedDate.Month >= 1 && p.CreatedDate.Month <= 3), // Q3 fiscal year: Jan-Mar
            Q2Value = profDev.Count(p => p.CreatedDate.Month >= 4 && p.CreatedDate.Month <= 6)  // Q4 fiscal year: Apr-Jun
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
            
            // Add comprehensive metrics to existing ones (avoid duplicates by name or similar functionality)
            foreach (var metric in comprehensiveMetrics)
            {
                // More sophisticated deduplication - check for similar metrics
                var existingMetric = goal.Metrics.FirstOrDefault(m => 
                    m.Name == metric.Name || 
                    (m.Name.Contains("Staff Satisfaction") && metric.Name.Contains("Staff Satisfaction")) ||
                    (m.Name.Contains("Professional Development") && metric.Name.Contains("Professional Development"))
                );
                
                if (existingMetric == null)
                {
                    goal.Metrics.Add(metric);
                }
                else
                {
                    // Update existing metric with comprehensive data if it has more complete information
                    if (!string.IsNullOrEmpty(metric.Target) && string.IsNullOrEmpty(existingMetric.Target))
                    {
                        existingMetric.Target = metric.Target;
                    }
                    if (!string.IsNullOrEmpty(metric.DataSource) && existingMetric.DataSource != metric.DataSource)
                    {
                        existingMetric.DataSource = $"{existingMetric.DataSource} + {metric.DataSource}";
                    }
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

        // 5. Issue Resolution (from Plan2026_24D - planIssue_25D table was merged into this table)
        var planIssues = await _context.Plan2026_24D
            .Where(p => !string.IsNullOrEmpty(p.IssueName)) // Only plans with issues
            .ToListAsync();
        var resolvedIssues = planIssues.Count(i => i.IssueHandled);
        
        AddOrUpdateMetric(goal, "Issue Resolution Rate", "Strategic issues addressed", 
            resolvedIssues, "resolved", "15", planIssues.Any() ? "Active" : "Planning",
            $"⚡ Issues Resolved: {resolvedIssues}/{planIssues.Count} | Form: Data Entry → 2026 Planning", nextId++);
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
        // Note: ProfessionalDevelopmentYear26/27 fields were removed - now we count total records and activities
        var totalActivities = profDevs.Sum(p => p.Activities.Split(',', ';').Length); // Approximate activity count
        
        AddOrUpdateMetric(goal, "Professional Development Plans", "Staff growth initiatives", 
            participatingEmployees, "employees", "25", profDevs.Any() ? "Active" : "Planning",
            profDevs.Any()
                ? $"{participatingEmployees} employees participating, ~{totalActivities} total activities | Form: Data Entry → Professional Development"
                : "No professional development yet - Go to Data Entry → Professional Development", nextId++);

        // 3. Board Member Recruitment
        var boardMembers = await _context.BoardMember_29D.ToListAsync();
        var totalRecruited = boardMembers.Sum(b => b.NumberRecruited);
        
        AddOrUpdateMetric(goal, "Board Recruitment", "New board member acquisition", 
            totalRecruited ?? 0, "members", "10", boardMembers.Any() ? "Active" : "Planning",
            boardMembers.Any() ? $"Board members recruited: {totalRecruited ?? 0}" : "No board recruitment data yet - Go to Data Entry → Board Management", nextId++);

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

        // 2. Fee-for-Service Revenue
        var feeServices = await _context.FeeForServices_21D.ToListAsync();
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
        
        if (existingMetric != null)
        {
            // Update existing metric
            existingMetric.CurrentValue = currentValue;
            existingMetric.Status = status;
            existingMetric.Description = detailedDescription;
            existingMetric.Target = target;
            existingMetric.Unit = unit;
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
