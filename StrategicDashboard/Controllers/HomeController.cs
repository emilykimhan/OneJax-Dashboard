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
            PopulateDashboardComputedValues(dashboardData, selectedFiscalYear, overallGoals);
            dashboardData.StrategicGoals = overallGoals;

            // Apply status and time filters to the cumulative goal detail view.
            // Skip fiscal-year filtering here so the goal tabs remain all-years.
            dashboardData = ApplyFilters(
                dashboardData,
                status ?? "",
                time ?? "",
                goal ?? "",
                selectedFiscalYear,
                quarter ?? "",
                includeFiscalYearFilter: false);

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
                ViewBag.CommunityYouthEntryCount = youthRows.Count;

                ViewBag.CommunityYouthPrePostAvg = new
                {
                    pre = youthRows.Any() ? Math.Round(youthRows.Average(y => (double)y.AveragePreAssessment), 1) : 0.0,
                    post = youthRows.Any() ? Math.Round(youthRows.Average(y => (double)y.AveragePostAssessment), 1) : 0.0
                };
                ViewBag.CommunityYouthAverageSatisfaction = youthRows.Any()
                    ? (double?)Math.Round(youthRows.Average(y => (double)y.PostEventSurveySatisfaction), 1)
                    : null;

                if (youthRows.Count >= 2)
                {
                    var latestYouth = youthRows[^1];
                    var previousYouth = youthRows[^2];

                    if (previousYouth.NumberOfYouthAttendees > 0)
                    {
                        ViewBag.CommunityYouthGrowthPercent = Math.Round(
                            ((double)(latestYouth.NumberOfYouthAttendees - previousYouth.NumberOfYouthAttendees)
                            / previousYouth.NumberOfYouthAttendees) * 100, 1);
                    }
                }

                // Collaborative Partner Touchpoints (CollabTouch_47D): FY bar chart + latest 5 table (date + strategy only).
                var collabRows = await _context.CollabTouch_47D
                    .Include(c => c.Strategy)
                    .OrderByDescending(c => c.CreatedDate)
                    .ToListAsync();

                collabRows = collabRows
                    .Where(c => FiscalYearMatches(c.FiscalYear, selectedFiscalYear))
                    .ToList();

                ViewBag.CommunityPartnerEntryCount = collabRows.Count;
                ViewBag.CommunityPartnersCurrentCount = collabRows.Count;
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

                var interfaithRows = await _context.Interfaith_11D
                    .OrderByDescending(i => i.CreatedDate)
                    .ToListAsync();

                if (selectedFiscalYearRange != null)
                {
                    interfaithRows = interfaithRows
                        .Where(i => i.CreatedDate >= selectedFiscalYearRange.Value.StartDate && i.CreatedDate <= selectedFiscalYearRange.Value.EndDate)
                        .ToList();
                }

                ViewBag.CommunityInterfaithEntryCount = interfaithRows.Count;
                ViewBag.CommunityInterfaithEventsCount = interfaithRows.Count;

                var faithRows = await _context.FaithCommunity_13D
                    .OrderByDescending(f => f.CreatedDate)
                    .ToListAsync();

                if (selectedFiscalYearRange != null)
                {
                    faithRows = faithRows
                        .Where(f => f.CreatedDate >= selectedFiscalYearRange.Value.StartDate && f.CreatedDate <= selectedFiscalYearRange.Value.EndDate)
                        .ToList();
                }

                var faithEventsMeetingGoal = faithRows.Count(f => f.NumberOfFaithsRepresented >= 3);
                ViewBag.CommunityFaithEntryCount = faithRows.Count;
                ViewBag.CommunityFaithTotalEvents = faithRows.Count;
                ViewBag.CommunityFaithEventsMeetingGoal = faithEventsMeetingGoal;
                ViewBag.CommunityFaithPercentMeetingGoal = faithRows.Count > 0
                    ? (double?)Math.Round((double)faithEventsMeetingGoal / faithRows.Count * 100, 1)
                    : null;

                var satisfactionRows = await _context.EventSatisfaction_12D
                    .OrderByDescending(s => s.CreatedDate)
                    .ToListAsync();

                if (selectedFiscalYearRange != null)
                {
                    satisfactionRows = satisfactionRows
                        .Where(s => s.CreatedDate >= selectedFiscalYearRange.Value.StartDate && s.CreatedDate <= selectedFiscalYearRange.Value.EndDate)
                        .ToList();
                }

                ViewBag.CommunitySatisfactionEntryCount = satisfactionRows.Count;
                ViewBag.CommunityEventSatisfactionAvg = satisfactionRows.Any()
                    ? (double?)Math.Round(satisfactionRows.Average(s => (double)s.EventAttendeeSatisfactionPercentage), 1)
                    : null;

                var contactRows = await _context.ContactsInterfaith_14D
                    .OrderBy(c => c.Year)
                    .ToListAsync();

                contactRows = FilterByFiscalYearYears(contactRows, selectedFiscalYear, c => c.Year)
                    .OrderBy(c => c.Year)
                    .ToList();

                ViewBag.CommunityContactEntryCount = contactRows.Count;
                ViewBag.CommunityClergyLatestTotal = contactRows.LastOrDefault()?.TotalInterfaithContacts ?? 0;
                if (contactRows.Count >= 2)
                {
                    var latestContacts = contactRows[^1];
                    var previousContacts = contactRows[^2];

                    if (previousContacts.TotalInterfaithContacts > 0)
                    {
                        ViewBag.CommunityClergyGrowthPercent = Math.Round(
                            ((double)(latestContacts.TotalInterfaithContacts - previousContacts.TotalInterfaithContacts)
                            / previousContacts.TotalInterfaithContacts) * 100, 1);
                    }
                }

                var diversityRows = await _context.Diversity_37D
                    .OrderByDescending(d => d.CreatedDate)
                    .ToListAsync();

                diversityRows = diversityRows
                    .Where(d => FiscalYearMatches(d.FiscalYear, selectedFiscalYear))
                    .OrderByDescending(d => d.CreatedDate)
                    .ToList();

                ViewBag.CommunityDiversityEntryCount = diversityRows.Count;
                ViewBag.CommunityDiversityLatestCount = diversityRows.FirstOrDefault()?.DiversityCount ?? 0;
                if (diversityRows.Count >= 2)
                {
                    var latestDiversity = diversityRows[0];
                    var previousDiversity = diversityRows[1];

                    if (previousDiversity.DiversityCount > 0)
                    {
                        ViewBag.CommunityDiversityGrowthPercent = Math.Round(
                            ((double)(latestDiversity.DiversityCount - previousDiversity.DiversityCount)
                            / previousDiversity.DiversityCount) * 100, 1);
                    }
                }

                var firstTimeRows = await _context.FirstTime_38D
                    .OrderByDescending(f => f.CreatedDate)
                    .ToListAsync();

                firstTimeRows = firstTimeRows
                    .Where(f => FiscalYearMatches(f.FiscalYear, selectedFiscalYear))
                    .OrderByDescending(f => f.CreatedDate)
                    .ToList();

                ViewBag.CommunityFirstTimeEntryCount = firstTimeRows.Count;
                ViewBag.CommunityFirstTimeTotalParticipants = firstTimeRows.Sum(f => f.NumberOfFirstTimeParticipants);
                ViewBag.CommunityFirstTimeTotalAttendees = firstTimeRows.Sum(f => f.TotalAttendees);
                ViewBag.CommunityFirstTimeEventsMeetingGoal = firstTimeRows.Count(f => f.GoalMet);
                ViewBag.CommunityFirstTimeRateAvg = firstTimeRows.Any()
                    ? (double?)Math.Round(firstTimeRows.Average(f => (double)f.FirstTimeParticipantRate), 1)
                    : null;
            }
            catch { }

            // Add board meeting attendance records for the enhanced view
            var boardMeetingAttendance = await _context.BoardMeetingAttendance
                .OrderByDescending(b => b.MeetingDate)
                .ToListAsync();
            var filteredBoardMeetingAttendance = FilterByFiscalYearDate(boardMeetingAttendance, selectedFiscalYear, b => b.MeetingDate)
                .OrderByDescending(b => b.MeetingDate)
                .ToList();
            var boardMeetingAttendanceAverage = filteredBoardMeetingAttendance
                .Where(b => (b.TotalBoardMembers ?? 0) > 0)
                .Select(b => (decimal)b.MembersInAttendance / (b.TotalBoardMembers ?? 0) * 100m)
                .DefaultIfEmpty(0m)
                .Average();

            ViewBag.BoardMeetingAttendanceAverage = Math.Round(boardMeetingAttendanceAverage, 1);
            ViewBag.BoardMeetingAttendanceCount = filteredBoardMeetingAttendance.Count;
            ViewBag.BoardMeetingAttendance = filteredBoardMeetingAttendance
                .OrderByDescending(b => b.MeetingDate)
                .Take(10)
                .ToList();

            var staffSurveyRecords = FilterByFiscalYearDate(
                    await _context.StaffSurveys_22D
                        .OrderByDescending(s => s.CreatedDate)
                        .ToListAsync(),
                    selectedFiscalYear,
                    s => s.CreatedDate)
                .OrderByDescending(s => s.CreatedDate)
                .ToList();
            ViewBag.StaffSurveyRecords = staffSurveyRecords;
            ViewBag.StaffSurveyAverage = staffSurveyRecords.Any()
                ? Math.Round(staffSurveyRecords.Average(s => s.SatisfactionRate), 1)
                : 0d;

            var boardSelfAssessmentRecords = FilterByFiscalYearYearsWithCreatedDateFallback(
                    await _context.selfAssess_31D
                        .OrderByDescending(a => a.Year)
                        .ThenByDescending(a => a.CreatedDate)
                        .ToListAsync(),
                    selectedFiscalYear,
                    a => a.Year,
                    a => a.CreatedDate)
                .OrderByDescending(a => a.Year)
                .ThenByDescending(a => a.CreatedDate)
                .ToList();
            ViewBag.BoardSelfAssessmentRecords = boardSelfAssessmentRecords;
            ViewBag.BoardSelfAssessmentAverage = boardSelfAssessmentRecords.Any()
                ? Math.Round(boardSelfAssessmentRecords.Average(a => a.SelfAssessmentScore), 1)
                : 0d;

            var professionalDevelopmentRecords = FilterByFiscalYearYearsWithCreatedDateFallback(
                    await _context.ProfessionalDevelopments
                        .OrderByDescending(p => p.Year)
                        .ThenByDescending(p => p.CreatedDate)
                        .ToListAsync(),
                    selectedFiscalYear,
                    p => p.Year,
                    p => p.CreatedDate)
                .OrderByDescending(p => p.Year)
                .ThenByDescending(p => p.CreatedDate)
                .ToList();
            ViewBag.ProfessionalDevelopmentRecords = professionalDevelopmentRecords;
            ViewBag.ProfessionalDevelopmentParticipantCount = professionalDevelopmentRecords.Count;
            ViewBag.ProfessionalDevelopmentAccountCount = await _context.Staffauth
                .CountAsync(staff => !string.IsNullOrWhiteSpace(staff.Username));

            // Add board recruitment records for detailed organizational card display
            var boardRecruitmentRecords = await _context.BoardMember_29D
                .OrderByDescending(b => b.Year)
                .ThenByDescending(b => b.Quarter)
                .ThenByDescending(b => b.CreatedDate)
                .ToListAsync();
            ViewBag.BoardRecruitmentRecords = FilterByFiscalYearCalendarQuarterWithCreatedDateFallback(
                    boardRecruitmentRecords,
                    selectedFiscalYear,
                    b => b.Year,
                    b => b.Quarter,
                    b => b.CreatedDate)
                .OrderByDescending(b => b.Year)
                .ThenByDescending(b => b.Quarter)
                .ThenByDescending(b => b.CreatedDate)
                .ToList();

            // Add budget tracking data for Financial Sustainability chart
            var budgetTracking = await _context.BudgetTracking_28D.ToListAsync();
            if (TryParseFiscalYearEnd(selectedFiscalYear, out var budgetFiscalYearEnd))
            {
                var fiscalYearBudgetTracking = budgetTracking
                    .Where(b => b.Year == budgetFiscalYearEnd)
                    .ToList();
                budgetTracking = fiscalYearBudgetTracking;
            }
            ViewBag.BudgetTracking = budgetTracking;

            // Add volunteer program records for detailed organizational card display
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

            ViewBag.VolunteerProgramRecords = filteredVolunteerProgramRecords
                .Take(12)
                .ToList();
            ViewBag.VolunteerProgramAllFilteredRecords = filteredVolunteerProgramRecords;

            var volunteerTargetYearCount = 1;
            if (!TryParseFiscalYearEnd(selectedFiscalYear, out _)
                && filteredVolunteerProgramRecords.Any())
            {
                var currentFiscalYearEnd = GetFiscalYearEndFromDate(DateTime.Today);
                var earliestVolunteerFiscalYearEnd = filteredVolunteerProgramRecords
                    .Select(v => GetFiscalYearEndFromCalendarQuarterRecord(v.Year, v.Quarter)
                        ?? GetFiscalYearEndFromDate(v.CreatedDate))
                    .DefaultIfEmpty(currentFiscalYearEnd)
                    .Min();

                volunteerTargetYearCount = Math.Max(1, currentFiscalYearEnd - earliestVolunteerFiscalYearEnd + 1);
            }

            ViewBag.VolunteerProgramTargetYearCount = volunteerTargetYearCount;

            dashboardData.Summary.TotalEvents = dashboardData.StrategicGoals?.Sum(g => g.Events?.Count ?? 0) ?? 0;
            dashboardData.Summary.TotalActivities = dashboardData.Summary.TotalStaffSurveys +
                                                   dashboardData.Summary.TotalProfessionalDevelopmentPlans +
                                                   dashboardData.Summary.TotalMediaPlacements +
                                                   dashboardData.Summary.TotalWebsiteTrafficEntries +
                                                   dashboardData.Summary.TotalEvents;

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

    private DashboardViewModel ApplyFilters(
        DashboardViewModel dashboard,
        string status,
        string time,
        string goal,
        string fiscalYear,
        string quarter,
        bool includeFiscalYearFilter = true)
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
                        var fiscalYearMatch = !includeFiscalYearFilter
                            || fiscalYearRange == null
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

    private static void PopulateDashboardComputedValues(
        DashboardViewModel dashboard,
        string activeFiscalYear,
        IReadOnlyCollection<StrategicGoal> overallGoals)
    {
        var goals = dashboard.StrategicGoals?.ToList() ?? new List<StrategicGoal>();

        foreach (var goal in goals)
        {
            var goalMetrics = goal.Metrics ?? new List<GoalMetric>();
            var goalScheduledMetrics = DashboardMetricRules.ScheduledMetrics(goalMetrics, activeFiscalYear).ToList();
            var goalReportingMetrics = DashboardMetricRules.ReportingMetrics(goalMetrics, activeFiscalYear).ToList();

            goal.ScheduledMetricCount = goalScheduledMetrics.Count;
            goal.ReportingMetricCount = goalReportingMetrics.Count;
            goal.Progress = DashboardMetricRules.CalculateGoalProgress(goalMetrics, activeFiscalYear);
        }

        var overallScheduledMetrics = new List<GoalMetric>();
        var overallReportingMetrics = new List<GoalMetric>();
        var overallProgressMetrics = new List<GoalMetric>();

        foreach (var overallGoal in overallGoals)
        {
            var overallGoalMetrics = overallGoal.Metrics ?? new List<GoalMetric>();
            var scheduledMetrics = DashboardMetricRules.ScheduledMetrics(overallGoalMetrics, string.Empty).ToList();
            var reportingMetrics = DashboardMetricRules.ReportingMetrics(overallGoalMetrics, string.Empty).ToList();
            var progressMetrics = DashboardMetricRules.OverallProgressMetrics(overallGoalMetrics, string.Empty).ToList();

            overallGoal.OverallScheduledMetricCount = scheduledMetrics.Count;
            overallGoal.OverallReportingMetricCount = reportingMetrics.Count;
            overallGoal.OverallProgress = DashboardMetricRules.CalculateGoalProgress(overallGoalMetrics, string.Empty);
            overallGoal.OverallEventCount = overallGoal.Events?.Count ?? 0;

            overallScheduledMetrics.AddRange(scheduledMetrics);
            overallReportingMetrics.AddRange(reportingMetrics);
            overallProgressMetrics.AddRange(progressMetrics);

            var matchingGoal = goals.FirstOrDefault(goal => goal.Id == overallGoal.Id);
            if (matchingGoal != null)
            {
                matchingGoal.OverallScheduledMetricCount = overallGoal.OverallScheduledMetricCount;
                matchingGoal.OverallReportingMetricCount = overallGoal.OverallReportingMetricCount;
                matchingGoal.OverallProgress = overallGoal.OverallProgress;
                matchingGoal.OverallEventCount = overallGoal.OverallEventCount;
            }
        }

        dashboard.TotalMetricsMeetingGoal = overallProgressMetrics.Count(metric =>
            DashboardMetricRules.IsReportingMetric(metric, string.Empty)
            && DashboardMetricRules.IsMetricAtTarget(metric));
        dashboard.TotalEligibleMetrics = overallProgressMetrics.Count;
        dashboard.OverallDashboardProgress = overallGoals
            .Where(g => g.OverallScheduledMetricCount > 0)
            .Select(g => g.OverallProgress)
            .DefaultIfEmpty(0m)
            .Average();
        dashboard.PublicEventsCount = overallGoals.Sum(g => g.OverallEventCount);
        dashboard.InScopeMetricsCount = overallScheduledMetrics.Count;
        dashboard.ReportingMetricsCount = overallReportingMetrics.Count;
        dashboard.ActiveMetricsCount = overallScheduledMetrics.Count(m => string.Equals(m.Status, "Active", StringComparison.OrdinalIgnoreCase));
        dashboard.MetricsAtTargetCount = dashboard.TotalMetricsMeetingGoal;
        dashboard.ReportingMetricsPercentage = dashboard.InScopeMetricsCount > 0
            ? dashboard.ReportingMetricsCount * 100.0 / dashboard.InScopeMetricsCount
            : 0;
        dashboard.ActiveMetricsPercentage = dashboard.InScopeMetricsCount > 0
            ? dashboard.ActiveMetricsCount * 100.0 / dashboard.InScopeMetricsCount
            : 0;
        dashboard.MetricsAtTargetPercentage = dashboard.InScopeMetricsCount > 0
            ? dashboard.MetricsAtTargetCount * 100.0 / dashboard.InScopeMetricsCount
            : 0;
        dashboard.Charts.GoalProgress = new GoalProgressData
        {
            OrganizationalProgress = overallGoals.FirstOrDefault(g => g.Name.Contains("Organizational", StringComparison.OrdinalIgnoreCase))?.OverallProgress ?? 0,
            FinancialProgress = overallGoals.FirstOrDefault(g => g.Name.Contains("Financial", StringComparison.OrdinalIgnoreCase))?.OverallProgress ?? 0,
            IdentityProgress = overallGoals.FirstOrDefault(g => g.Name.Contains("Identity", StringComparison.OrdinalIgnoreCase))?.OverallProgress ?? 0,
            CommunityProgress = overallGoals.FirstOrDefault(g => g.Name.Contains("Community", StringComparison.OrdinalIgnoreCase))?.OverallProgress ?? 0
        };
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
        else if (normalized.StartsWith("FY", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized.Substring(2).Trim();
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

    private (DateTime StartDate, DateTime EndDate)? GetFiscalYearRangeOrNull(string fiscalYear)
    {
        return TryParseFiscalYearEnd(fiscalYear, out var fiscalYearEnd)
            ? GetFiscalYearRange(fiscalYearEnd)
            : ((DateTime StartDate, DateTime EndDate)?)null;
    }

    private static int GetFiscalYearEndFromDate(DateTime date)
    {
        return date.Month >= 7 ? date.Year + 1 : date.Year;
    }

    private static int? GetFiscalYearEndFromStoredYear(int year)
    {
        return year > 0 ? year : null;
    }

    private static int? GetFiscalYearEndFromCalendarQuarterRecord(int year, int quarter)
    {
        if (year <= 0 || quarter is < 1 or > 4)
        {
            return null;
        }

        // These forms store calendar quarters:
        // Q1 = Jan-Mar, Q2 = Apr-Jun, Q3 = Jul-Sep, Q4 = Oct-Dec.
        // Convert those calendar-quarter records into the fiscal year ending year
        // for OneJax's July 1 - June 30 fiscal year.
        return quarter switch
        {
            1 or 2 => year,
            3 or 4 => year + 1,
            _ => null
        };
    }

    private List<T> FilterByResolvedFiscalYear<T>(
        IEnumerable<T> source,
        string fiscalYear,
        Func<T, int?> fiscalYearEndSelector)
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
        return FilterByResolvedFiscalYear(
            source,
            fiscalYear,
            item => GetFiscalYearEndFromDate(dateSelector(item)));
    }

    private List<T> FilterByFiscalYearYears<T>(IEnumerable<T> source, string fiscalYear, Func<T, int> yearSelector)
    {
        return FilterByResolvedFiscalYear(
            source,
            fiscalYear,
            item => GetFiscalYearEndFromStoredYear(yearSelector(item)));
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

    private List<T> FilterByFiscalYearYearsWithCreatedDateFallback<T>(
        IEnumerable<T> source,
        string fiscalYear,
        Func<T, int> yearSelector,
        Func<T, DateTime> createdDateSelector)
    {
        return FilterByResolvedFiscalYear(
            source,
            fiscalYear,
            item => GetFiscalYearEndFromStoredYear(yearSelector(item))
                ?? GetFiscalYearEndFromDate(createdDateSelector(item)));
    }

    private bool FiscalYearMatches(string? recordFiscalYear, string selectedFiscalYear)
    {
        if (string.IsNullOrWhiteSpace(selectedFiscalYear))
        {
            return true;
        }

        return TryParseFiscalYearEnd(selectedFiscalYear, out var selectedFiscalYearEnd)
            && TryParseFiscalYearEnd(recordFiscalYear ?? string.Empty, out var recordFiscalYearEnd)
            && selectedFiscalYearEnd == recordFiscalYearEnd;
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
        dashboard.Summary = BuildDashboardSummary(string.Empty);
        
        // Build recent activities from real data
        dashboard.RecentActivities = BuildRecentActivities(string.Empty);

        // Build chart data from real metrics
        dashboard.Charts = BuildChartData(allGoals, string.Empty);
        
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

    private IQueryable<Strategy> GetVisibleDashboardStrategiesQuery()
    {
        return _context.Strategies
            .Where(s => !s.IsArchived)
            .Where(s => s.StrategicGoalId >= 1 && s.StrategicGoalId <= 4);
    }

    private async Task<List<Event>> GetDashboardEventsForGoalAsync(int strategicGoalId)
    {
        var strategies = await GetVisibleDashboardStrategiesQuery()
            .Where(s => s.StrategicGoalId == strategicGoalId)
            .OrderBy(s => string.IsNullOrWhiteSpace(s.Date))
            .ThenBy(s => s.Date)
            .Take(12)
            .ToListAsync();

        if (!strategies.Any())
        {
            return new List<Event>();
        }

        var strategyIds = strategies
            .Select(s => s.Id)
            .ToList();

        var linkedEvents = await _context.Events
            .Where(e => !e.IsArchived && e.StrategyId.HasValue && strategyIds.Contains(e.StrategyId.Value))
            .ToListAsync();

        var linkedEventsByStrategyId = linkedEvents
            .GroupBy(e => e.StrategyId!.Value)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(e => e.IsAssignedByAdmin ? 1 : 0)
                    .ThenBy(e => string.IsNullOrWhiteSpace(e.OwnerUsername) ? 0 : 1)
                    .ThenByDescending(e => e.Id)
                    .First());

        return strategies
            .Select(strategy => BuildDashboardEvent(strategy,
                linkedEventsByStrategyId.TryGetValue(strategy.Id, out var linkedEvent) ? linkedEvent : null))
            .OrderBy(e => e.DueDate == null)
            .ThenBy(e => e.DueDate)
            .ToList();
    }

    private static Event BuildDashboardEvent(Strategy strategy, Event? linkedEvent)
    {
        var strategyDate = ParseStrategyDate(strategy.Date);

        return new Event
        {
            Id = linkedEvent?.Id ?? 0,
            StrategyTemplateId = strategy.Id,
            StrategyId = strategy.Id,
            StrategicGoalId = strategy.StrategicGoalId,
            Title = linkedEvent?.Title ?? strategy.Name,
            Description = linkedEvent?.Description ?? strategy.Description,
            Status = !string.IsNullOrWhiteSpace(linkedEvent?.Status) ? linkedEvent.Status : "Planned",
            Type = !string.IsNullOrWhiteSpace(linkedEvent?.Type) ? linkedEvent.Type : (strategy.ProgramType ?? "Program"),
            Location = linkedEvent?.Location ?? string.Empty,
            Attendees = linkedEvent?.Attendees ?? 0,
            Notes = linkedEvent?.Notes ?? string.Empty,
            StartDate = linkedEvent?.StartDate,
            EndDate = linkedEvent?.EndDate,
            DueDate = linkedEvent?.DueDate ?? strategyDate,
            OwnerUsername = linkedEvent?.OwnerUsername ?? string.Empty,
            IsAssignedByAdmin = linkedEvent?.IsAssignedByAdmin ?? false,
            AssignmentDate = linkedEvent?.AssignmentDate,
            AdminNotes = linkedEvent?.AdminNotes ?? string.Empty,
            SatisfactionScore = linkedEvent?.SatisfactionScore,
            PreAssessmentData = linkedEvent?.PreAssessmentData ?? string.Empty,
            PostAssessmentData = linkedEvent?.PostAssessmentData ?? string.Empty,
            IsArchived = false,
            CompletionDate = linkedEvent?.CompletionDate
        };
    }

    private static DateTime? ParseStrategyDate(string? strategyDate)
    {
        if (string.IsNullOrWhiteSpace(strategyDate))
        {
            return null;
        }

        return DateTime.TryParse(strategyDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate)
            ? parsedDate
            : null;
    }

    private DashboardSummary BuildDashboardSummary(string fiscalYear)
    {
        var summary = new DashboardSummary();
        
        try
        {
            // Staff Surveys
            var staffSurveys = FilterByFiscalYearDate(_context.StaffSurveys_22D.ToList(), fiscalYear, s => s.CreatedDate);
            summary.TotalStaffSurveys = staffSurveys.Count;
            summary.AverageStaffSatisfaction = staffSurveys.Any() ? 
                (decimal)staffSurveys.Average(s => s.SatisfactionRate) : 0;

            // Professional Development
            summary.TotalProfessionalDevelopmentPlans = FilterByFiscalYearYearsWithCreatedDateFallback(
                _context.ProfessionalDevelopments.ToList(),
                fiscalYear,
                p => p.Year,
                p => p.CreatedDate).Count;

            // Media Placements
            summary.TotalMediaPlacements = FilterByFiscalYearDate(
                _context.MediaPlacements_3D.ToList(),
                fiscalYear,
                m => m.CreatedDate).Count;

            // Website Traffic
            summary.TotalWebsiteTrafficEntries = FilterByFiscalYearDate(
                _context.WebsiteTraffic.ToList(),
                fiscalYear,
                w => w.CreatedDate).Count;

            // Events (real Events table only)
            try
            {
                var visibleStrategies = GetVisibleDashboardStrategiesQuery().ToList();
                var fiscalYearRange = GetFiscalYearRangeOrNull(fiscalYear);
                if (fiscalYearRange != null)
                {
                    visibleStrategies = visibleStrategies
                        .Where(s =>
                        {
                            var strategyDate = ParseStrategyDate(s.Date);
                            return !strategyDate.HasValue
                                || (strategyDate.Value >= fiscalYearRange.Value.StartDate && strategyDate.Value <= fiscalYearRange.Value.EndDate);
                        })
                        .ToList();
                }

                summary.TotalEvents = visibleStrategies.Count;
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

    private List<RecentActivity> BuildRecentActivities(string fiscalYear)
    {
        var activities = new List<RecentActivity>();

        try
        {
            // Add recent staff surveys
            var recentSurveys = FilterByFiscalYearDate(_context.StaffSurveys_22D.ToList(), fiscalYear, s => s.CreatedDate)
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
            var recentProfDev = FilterByFiscalYearYearsWithCreatedDateFallback(
                    _context.ProfessionalDevelopments.ToList(),
                    fiscalYear,
                    p => p.Year,
                    p => p.CreatedDate)
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
            var recentMedia = FilterByFiscalYearDate(_context.MediaPlacements_3D.ToList(), fiscalYear, m => m.CreatedDate)
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
            var recentTraffic = FilterByFiscalYearDate(_context.WebsiteTraffic.ToList(), fiscalYear, w => w.CreatedDate)
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

            // Add recent dashboard events from core strategies
            try 
            {
                var recentStrategies = GetVisibleDashboardStrategiesQuery().ToList();
                var fiscalYearRange = GetFiscalYearRangeOrNull(fiscalYear);
                if (fiscalYearRange != null)
                {
                    recentStrategies = recentStrategies
                        .Where(s =>
                        {
                            var strategyDate = ParseStrategyDate(s.Date);
                            return !strategyDate.HasValue
                                || (strategyDate.Value >= fiscalYearRange.Value.StartDate && strategyDate.Value <= fiscalYearRange.Value.EndDate);
                        })
                        .ToList();
                }

                recentStrategies = recentStrategies
                    .OrderByDescending(s => s.Id)
                    .Take(3)
                    .ToList();

                var recentStrategyIds = recentStrategies
                    .Select(s => s.Id)
                    .ToList();

                var recentLinkedEvents = _context.Events
                    .Where(e => !e.IsArchived && e.StrategyId.HasValue && recentStrategyIds.Contains(e.StrategyId.Value))
                    .ToList();

                var recentLinkedEventsByStrategyId = recentLinkedEvents
                    .GroupBy(e => e.StrategyId!.Value)
                    .ToDictionary(
                        group => group.Key,
                        group => group
                            .OrderBy(e => e.IsAssignedByAdmin ? 1 : 0)
                            .ThenBy(e => string.IsNullOrWhiteSpace(e.OwnerUsername) ? 0 : 1)
                            .ThenByDescending(e => e.Id)
                            .First());

                var recentEvents = recentStrategies
                    .Select(strategy => BuildDashboardEvent(strategy,
                        recentLinkedEventsByStrategyId.TryGetValue(strategy.Id, out var linkedEvent) ? linkedEvent : null))
                    .OrderByDescending(e => e.StartDate ?? e.DueDate ?? DateTime.MinValue)
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

    private async Task<DashboardViewModel> BuildEnhancedDashboardAsync(string? fiscalYear = null)
    {
        fiscalYear ??= GetCurrentFiscalYearLabel();
        var dashboard = new DashboardViewModel();
        
        // Always create all four strategic goals (regardless of data)
        var allGoals = await CreateAllStrategicGoalsAsync();
        
        // Enhance each strategic goal with comprehensive metrics from MetricsService
        await EnhanceGoalsWithComprehensiveMetricsAsync(allGoals, fiscalYear);
        
        // Build summary statistics from real data
        dashboard.Summary = BuildDashboardSummary(fiscalYear);
        
        // Build recent activities from real data
        dashboard.RecentActivities = BuildRecentActivities(fiscalYear);

        // Build chart data from real metrics
        dashboard.Charts = BuildChartData(allGoals, fiscalYear);
        
        // Always show all goals with their metrics (even if empty)
        dashboard.StrategicGoals = allGoals;
        dashboard.DataSource = "Comprehensive Metrics + Real Data Integration";
        dashboard.Message = "Dashboard shows all strategic goals with comprehensive metrics. Metrics update in real-time as you submit data through the Data Entry forms.";

        // Identity: ZIP coverage (drives Programs Demographics map)
        dashboard.ZipCoverage = await BuildZipCoverageAsync(fiscalYear);
        dashboard.Identity = await BuildIdentityDashboardDataAsync(dashboard.ZipCoverage, fiscalYear);
        
        return dashboard;
    }

    private static string GetCurrentFiscalYearLabel(DateTime? currentDate = null)
    {
        var date = currentDate ?? DateTime.Today;
        var startYear = date.Month >= 7 ? date.Year : date.Year - 1;
        var endYear = startYear + 1;
        return $"{startYear}-{endYear}";
    }

    private static List<string> GetFiscalYearOptions(string centerFiscalYear, int yearsBefore, int yearsAfter)
    {
        if (!TryParseFiscalYearLabel(centerFiscalYear, out var startYear, out _))
        {
            centerFiscalYear = GetCurrentFiscalYearLabel();
            TryParseFiscalYearLabel(centerFiscalYear, out startYear, out _);
        }

        return Enumerable.Range(startYear - yearsBefore, yearsBefore + yearsAfter + 1)
            .Select(year => $"{year}-{year + 1}")
            .ToList();
    }

    private static bool TryParseFiscalYearLabel(string? fiscalYear, out int startYear, out int endYear)
    {
        startYear = 0;
        endYear = 0;

        if (string.IsNullOrWhiteSpace(fiscalYear))
        {
            return false;
        }

        var normalized = fiscalYear.Trim();
        if (normalized.StartsWith("FY ", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized.Substring(3).Trim();
        }
        else if (normalized.StartsWith("FY", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized.Substring(2).Trim();
        }

        var parts = normalized.Split(new[] { '-', '/' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2
            || !int.TryParse(parts[0], out startYear)
            || !int.TryParse(parts[1], out endYear))
        {
            return false;
        }

        return endYear == startYear + 1;
    }

    private async Task<IdentityDashboardData> BuildIdentityDashboardDataAsync(Dictionary<string, int> zipCoverage, string fiscalYear)
    {
        var data = new IdentityDashboardData
        {
            ZipCodesServed = zipCoverage?.Count ?? 0
        };

        // Media Placements
        try
        {
            var all = FilterByFiscalYearDate(await _context.MediaPlacements_3D.ToListAsync(), fiscalYear, m => m.CreatedDate);
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
            var all = FilterByFiscalYearDate(await _context.WebsiteTraffic.ToListAsync(), fiscalYear, w => w.CreatedDate);
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
            var surveys = FilterByFiscalYearYears(await _context.Annual_average_7D.ToListAsync(), fiscalYear, s => s.Year);
            var latest = surveys
                .OrderByDescending(s => s.Year)
                .ThenByDescending(s => s.CreatedDate)
                .FirstOrDefault();
            data.TrustPercent = latest?.Percentage ?? 0m;
            data.TrustRespondents = latest?.TotalRespondents;
            data.TrustYear = latest?.Year;
            data.TrustLastUpdated = latest?.CreatedDate;

            var history = surveys
                .OrderByDescending(s => s.Year)
                .ThenByDescending(s => s.CreatedDate)
                .Take(5)
                .ToList();
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
            var latest = FilterByFiscalYearDate(await _context.achieveMile_6D.ToListAsync(), fiscalYear, m => m.CreatedDate)
                .OrderByDescending(m => m.CreatedDate)
                .FirstOrDefault();
            data.MilestonePercent = latest?.Percentage ?? 0m;
            data.MilestoneReviewActive = latest?.achievedReview ?? false;
            data.MilestoneLastUpdated = latest?.CreatedDate;
        }
        catch { }

        // Social Media Engagement
        try
        {
            var latest = FilterByFiscalYearYears(await _context.socialMedia_5D.ToListAsync(), fiscalYear, s => s.Year)
                .OrderByDescending(s => s.Year)
                .ThenByDescending(s => s.CreatedDate)
                .FirstOrDefault();
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
            var latest = FilterByFiscalYearYears(await _context.Plan2026_24D.ToListAsync(), fiscalYear, p => p.Year)
                .OrderByDescending(p => p.Year)
                .ThenByDescending(p => p.Quarter)
                .ThenByDescending(p => p.CreatedDate)
                .FirstOrDefault();
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

    private async Task<Dictionary<string, int>> BuildZipCoverageAsync(string fiscalYear)
    {
        var coverage = new Dictionary<string, int>(StringComparer.Ordinal);

        List<demographics_8D> rows;
        try
        {
            rows = FilterByFiscalYearYears(
                await _context.demographics_8D
                    .OrderByDescending(d => d.CreatedDate)
                    .ToListAsync(),
                fiscalYear,
                d => d.Year);
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
        foreach (var goal in goals)
        {
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
                await AddIdentityMetricsAsync(goal, fiscalYear);
            }
        
            // Organizational Building Goal - Staff, Professional Development, Board Management
            else if (goal.Name.Contains("Organizational"))
            {
                await AddOrganizationalMetricsAsync(goal, fiscalYear);
            }
        
            // Financial Sustainability Goal - Budget, Revenue, Donors, Fees
            else if (goal.Name.Contains("Financial"))
            {
                await AddFinancialMetricsAsync(goal, fiscalYear);
            }

            // Community Engagement Goal - Collaborations, Communications, Surveys, Programs
            else if (goal.Name.Contains("Community"))
            {
                await AddCommunityMetricsAsync(goal, fiscalYear);
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating metrics: {ex.Message}");
        }
    }

    private int? GetIncomeRecordYear(income_27D income)
    {
        if (income.Year.HasValue && income.Year.Value > 0)
        {
            if (!string.IsNullOrWhiteSpace(income.Month)
                && DateTime.TryParseExact(
                    $"{income.Month} {income.Year.Value}",
                    new[] { "MMMM yyyy", "MMM yyyy", "M yyyy" },
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var occurrenceDate))
            {
                return GetFiscalYearEndFromDate(occurrenceDate);
            }

            return income.Year.Value;
        }

        return income.CreatedDate.Year > 0 ? GetFiscalYearEndFromDate(income.CreatedDate) : null;
    }

    private async Task AddIdentityMetricsAsync(StrategicGoal goal, string fiscalYear)
    {
        var nextId = goal.Metrics.Count + 1000;

        // IDENTITY/VALUE PROPOSITION - Connect ALL relevant forms
        
        // 1. Media Placements (from MediaPlacements_3D table)
        var mediaPlacements = FilterByFiscalYearDate(await _context.MediaPlacements_3D.ToListAsync(), fiscalYear, m => m.CreatedDate);
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
            $"📺 Media Placements: {totalPlacements} | Form: Data Entry → Media Placements", nextId++, metricType: "Count", fiscalYear: fiscalYear);

        // 2. Website Traffic (from WebsiteTraffic table - FIXED table name)
        var websiteTraffic = FilterByFiscalYearDate(await _context.WebsiteTraffic.ToListAsync(), fiscalYear, w => w.CreatedDate);
        var totalTraffic = websiteTraffic.Sum(w => (w.Q1_JulySeptember ?? 0) + (w.Q2_OctoberDecember ?? 0) + 
                                                  (w.Q3_JanuaryMarch ?? 0) + (w.Q4_AprilJune ?? 0));
        
        AddOrUpdateMetric(goal, "Website Traffic", "Quarterly website engagement", 
            totalTraffic, "clicks", "10000", websiteTraffic.Any() ? "Active" : "Planning",
            $"🌐 Website Traffic: {totalTraffic:N0} clicks | Form: Data Entry → Website Traffic", nextId++, metricType: "Quarterly", fiscalYear: fiscalYear);
        var websiteMetric = goal.Metrics.FirstOrDefault(m => m.Name == "Website Traffic");
        if (websiteMetric != null)
        {
            websiteMetric.Q1Value = websiteTraffic.Sum(w => w.Q1_JulySeptember ?? 0);
            websiteMetric.Q2Value = websiteTraffic.Sum(w => w.Q2_OctoberDecember ?? 0);
            websiteMetric.Q3Value = websiteTraffic.Sum(w => w.Q3_JanuaryMarch ?? 0);
            websiteMetric.Q4Value = websiteTraffic.Sum(w => w.Q4_AprilJune ?? 0);
        }

        // 3. Geographic Reach (from demographics_8D)
        var demographics = FilterByFiscalYearYears(await _context.demographics_8D.ToListAsync(), fiscalYear, d => d.Year);
        var uniqueZipCodes = 0;
        if (demographics.Any())
        {
            var allZipCodes = demographics.SelectMany(d => d.ZipCodes.Split(',', StringSplitOptions.RemoveEmptyEntries))
                                        .Select(z => z.Trim()).Where(z => !string.IsNullOrEmpty(z)).Distinct();
            uniqueZipCodes = allZipCodes.Count();
        }
        
        AddOrUpdateMetric(goal, "Geographic Reach", "Service area expansion", 
            uniqueZipCodes, "ZIP codes", "25", demographics.Any() ? "Active" : "Planning",
            $"📍 ZIP Codes Served: {uniqueZipCodes} | Form: Data Entry → Demographics", nextId++, metricType: "Count", fiscalYear: fiscalYear);

        // Note: Brand Trust Rate moved to Community Engagement tab as "Community Trust Rating"
        // to avoid duplication and better align with data source context

        // 4. Community Perception Survey (from Annual_average_7D)
        var annualSurvey = FilterByFiscalYearYears(await _context.Annual_average_7D.ToListAsync(), fiscalYear, s => s.Year);
        var latestSurvey = annualSurvey.OrderByDescending(s => s.Year).FirstOrDefault();
        var trustRating = latestSurvey?.Percentage ?? 0;
        var trustRespondentCount = latestSurvey?.TotalRespondents ?? 0;
        
        AddOrUpdateMetric(goal, "Community Perception Survey", "Biannual survey - 70% trust rating target by Q4 2025", 
            trustRating, "%", "70", annualSurvey.Any() ? "Active" : "Planning",
            $"🌟 {trustRating}% identify OneJax as trusted leader ({latestSurvey?.TotalRespondents} respondents, {latestSurvey?.Year}) | Form: Data Entry → Annual Survey", nextId++, metricType: "Percentage", fiscalYear: fiscalYear,
            sampleCount: trustRespondentCount,
            sampleCountText: trustRespondentCount > 0 ? $"{trustRespondentCount} respondents in the latest survey" : string.Empty);

        // 5. Strategic Planning (from Plan2026_24D)
        var plan2026 = FilterByFiscalYearYears(await _context.Plan2026_24D.ToListAsync(), fiscalYear, p => p.Year);
        var completedPlans = plan2026.Count(p => p.GoalMet);
        
        AddOrUpdateMetric(goal, "Strategic Plan Completion", "2026 planning progress", 
            completedPlans, "goals met", "20", plan2026.Any() ? "Active" : "Planning",
            $"🎯 Plans Completed: {completedPlans}/{plan2026.Count} | Form: Data Entry → 2026 Planning", nextId++, metricType: "Count", fiscalYear: fiscalYear);
    }

    private async Task AddOrganizationalMetricsAsync(StrategicGoal goal, string fiscalYear)
    {
        var nextId = goal.Metrics.Count + 2000;

        // 1. Staff Satisfaction (from Staff Surveys form)
        var staffSurveys = FilterByFiscalYearDate(await _context.StaffSurveys_22D.ToListAsync(), fiscalYear, s => s.CreatedDate);
        var avgSatisfaction = staffSurveys.Any() ? staffSurveys.Average(s => s.SatisfactionRate) : 0;
        
        AddOrUpdateMetric(goal, "Staff Satisfaction Rating", "Annual Team Satisfaction Survey", 
            Math.Round((decimal)avgSatisfaction, 1), "%", "85", staffSurveys.Any() ? "Active" : "Planning",
            staffSurveys.Any()
                ? $"{staffSurveys.Count} staff surveys submitted, {avgSatisfaction:F1}% average satisfaction | Form: Data Entry → Staff Surveys"
                : "No staff surveys yet - Go to Data Entry → Staff Surveys", nextId++, metricType: "Percentage", fiscalYear: fiscalYear,
            sampleCount: staffSurveys.Count,
            sampleCountText: staffSurveys.Any() ? $"{staffSurveys.Count} survey entr{(staffSurveys.Count == 1 ? "y" : "ies")} averaged" : string.Empty);

        // 2. Professional Development (employee participation + activity totals)
        var profDevs = FilterByFiscalYearYearsWithCreatedDateFallback(
            await _context.ProfessionalDevelopments.ToListAsync(),
            fiscalYear,
            p => p.Year,
            p => p.CreatedDate);
        var participatingEmployees = profDevs.Count;
        var dashboardAccessAccountCount = await _context.Staffauth
            .CountAsync(staff => !string.IsNullOrWhiteSpace(staff.Username));
        
        AddOrUpdateMetric(goal, "Professional Development Plans", "Staff growth initiatives", 
            participatingEmployees, "employees", dashboardAccessAccountCount.ToString(CultureInfo.InvariantCulture), profDevs.Any() ? "Active" : "Planning",
            profDevs.Any()
                ? $"{participatingEmployees} employees participating out of {dashboardAccessAccountCount} dashboard accounts | Form: Data Entry → Professional Development"
                : $"No professional development yet. {dashboardAccessAccountCount} dashboard accounts currently have access | Go to Data Entry → Professional Development", nextId++, metricType: "Count", fiscalYear: fiscalYear);

        // 3. Board Member Recruitment
        var boardMembers = FilterByFiscalYearCalendarQuarterWithCreatedDateFallback(
            await _context.BoardMember_29D.ToListAsync(),
            fiscalYear,
            b => b.Year,
            b => b.Quarter,
            b => b.CreatedDate);
        var totalRecruited = boardMembers.Sum(b => b.NumberRecruited ?? 0);
        var totalProspectOutreach = boardMembers.Sum(b => b.TotalProspectOutreach);
        
        AddOrUpdateMetric(goal, "Board Recruitment", "New board member acquisition", 
            totalRecruited, "members", "6", boardMembers.Any() ? "Active" : "Planning",
            boardMembers.Any()
                ? $"Board members recruited: {totalRecruited}, total prospect outreach: {totalProspectOutreach} | Form: Data Entry → Board Management"
                : "No board recruitment data yet - Go to Data Entry → Board Management", nextId++, metricType: "Count", fiscalYear: fiscalYear);

        // 4. Board Meeting Attendance
        var boardAttendance = FilterByFiscalYearDate(await _context.BoardMeetingAttendance.ToListAsync(), fiscalYear, b => b.MeetingDate);
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
            boardAttendance.Any() ? $"Average attendance rate: {avgAttendanceRate:F1}%" : "No board attendance data yet - Go to Data Entry → Board Management", nextId++, metricType: "Percentage", fiscalYear: fiscalYear);

        // 5. Board Self-Assessment
        var boardSelfAssessments = FilterByFiscalYearYearsWithCreatedDateFallback(
            await _context.selfAssess_31D.ToListAsync(),
            fiscalYear,
            a => a.Year,
            a => a.CreatedDate);
        var avgBoardSelfAssessment = boardSelfAssessments.Any()
            ? boardSelfAssessments.Average(a => a.SelfAssessmentScore)
            : 0;

        AddOrUpdateMetric(goal, "Board Self-Assessment", "Average board annual self-assessment score",
            Math.Round((decimal)avgBoardSelfAssessment, 1), "%", "85", boardSelfAssessments.Any() ? "Active" : "Planning",
            boardSelfAssessments.Any()
                ? $"{boardSelfAssessments.Count} entries, {avgBoardSelfAssessment:F1}% average score | Form: Data Entry → Board Self-Assessment"
                : "No board self-assessment data yet - Go to Data Entry → Board Self-Assessment", nextId++, metricType: "Percentage", fiscalYear: fiscalYear,
            sampleCount: boardSelfAssessments.Count,
            sampleCountText: boardSelfAssessments.Any() ? $"{boardSelfAssessments.Count} assessment entr{(boardSelfAssessments.Count == 1 ? "y" : "ies")} averaged" : string.Empty);

        // 6. Volunteer Program
        var volunteerPrograms = FilterByFiscalYearCalendarQuarterWithCreatedDateFallback(
            await _context.volunteerProgram_40D.ToListAsync(),
            fiscalYear,
            v => v.Year,
            v => v.Quarter,
            v => v.CreatedDate);
        var totalVolunteers = volunteerPrograms.Sum(v => v.NumberOfVolunteers);
        var totalVolunteerInitiatives = volunteerPrograms.Sum(v => v.VolunteerLedInitiatives);

        AddOrUpdateMetric(goal, "Volunteer Program Participation", "Total volunteers and volunteer-led initiatives",
            totalVolunteers, "volunteers", "100", volunteerPrograms.Any() ? "Active" : "Planning",
            volunteerPrograms.Any()
                ? $"{totalVolunteers} volunteers across {volunteerPrograms.Count} entries, {totalVolunteerInitiatives} volunteer-led initiatives | Form: Data Entry → Volunteer Program"
                : "No volunteer program data yet - Go to Data Entry → Volunteer Program", nextId++, metricType: "Count", fiscalYear: fiscalYear);

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

        // 1. Budget Tracking
        var budgetTracking = await _context.BudgetTracking_28D.ToListAsync();
        if (hasFiscalYear)
        {
            var fiscalYearBudgetTracking = budgetTracking
                .Where(b => b.Year == fiscalYearEnd)
                .ToList();
            budgetTracking = fiscalYearBudgetTracking;
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
            budgetTracking.Any() ? $"Total revenue: ${totalRevenue:N0}, Total expenses: ${totalExpenses:N0}" : "No budget data yet - Go to Data Entry → Budget Tracking", nextId++, metricType: "Currency", fiscalYear: fiscalYear);

        // 2. Fee-for-Service Revenue
        var feeServices = FilterByFiscalYearDate(
            await _context.FeeForServices_21D.ToListAsync(),
            fiscalYear,
            f => f.WorkshopDate);
        var totalFeeRevenue = feeServices.Sum(f => f.RevenueReceived);
        var totalFeeExpenses = feeServices.Sum(f => f.ExpenseReceived);
        var totalFeeNetRevenue = totalFeeRevenue - totalFeeExpenses;
        var totalFeeServices = feeServices.Count;
        var totalFeeAttendees = feeServices.Sum(f => f.NumberOfAttendees);
        var avgRevenuePerService = totalFeeServices > 0 ? totalFeeNetRevenue / totalFeeServices : 0;
        
        AddOrUpdateMetric(goal, "Fee-for-Service Income", "Service-based revenue generation", 
            totalFeeNetRevenue, "dollars", "75000", feeServices.Any() ? "Active" : "Planning",
            feeServices.Any() ? $"Gross: ${totalFeeRevenue:N0}, Expenses: ${totalFeeExpenses:N0}, Net: ${totalFeeNetRevenue:N0} from {totalFeeServices} services, {totalFeeAttendees} attendees" : "No fee-for-service data yet - Go to Data Entry → Fee for Services", nextId++, metricType: "Currency", fiscalYear: fiscalYear);

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
                    return !effectiveYear.HasValue || effectiveYear.Value == fiscalYearEnd;
                })
                .ToList();
            incomeData = fiscalYearIncomeData;
        }
        var totalIncome = incomeData.Sum(i => i.Amount);
        
        AddOrUpdateMetric(goal, "General Income Streams", "Diversified income tracking", 
            totalIncome, "dollars", "100000", incomeData.Any() ? "Active" : "Planning",
            incomeData.Any() ? $"Total income: ${totalIncome:N0} from {incomeData.Count} sources" : "No income data yet - Go to Data Entry → Income Tracking", nextId++, metricType: "Currency", fiscalYear: fiscalYear);

        var incomeMetric = goal.Metrics.FirstOrDefault(m => m.Name == "General Income Streams");
        if (incomeMetric != null)
        {
            incomeMetric.Q1Value = incomeData.Count;
        }

        // 4. Donor Events
        var donorEvents = FilterByFiscalYearDate(await _context.DonorEvents_19D.ToListAsync(), fiscalYear, d => d.CreatedDate);
        var totalParticipants = donorEvents.Sum(d => d.NumberOfParticipants);
        var avgSatisfaction = donorEvents.Any() ? donorEvents.Average(d => d.EventSatisfactionRating) : 0;
        
        AddOrUpdateMetric(goal, "Donor Engagement Events", "Fundraising event effectiveness", 
            totalParticipants, "participants", "200", donorEvents.Any() ? "Active" : "Planning",
            donorEvents.Any() ? $"{donorEvents.Count} events, {totalParticipants} participants, {avgSatisfaction:F1}% avg satisfaction" : "No donor events yet - Go to Data Entry → Donor Events", nextId++, metricType: "Count", fiscalYear: fiscalYear);

        // Store donor event count for dashboard card visualizations
        var donorEngagementMetric = goal.Metrics.FirstOrDefault(m => m.Name == "Donor Engagement Events");
        if (donorEngagementMetric != null)
        {
            donorEngagementMetric.Q1Value = donorEvents.Count;
            donorEngagementMetric.Q2Value = Math.Round((decimal)avgSatisfaction, 1);
        }

        // 5. Donor Communication Satisfaction (from Communication Rate form)
        var commRate = FilterByFiscalYearDate(await _context.CommunicationRate.ToListAsync(), fiscalYear, c => c.CreatedDate);
        var avgCommSatisfaction = commRate.Any() ? commRate.Average(c => c.AverageCommunicationSatisfaction) : 0;

        AddOrUpdateMetric(goal, "Donor Communication Satisfaction", "Annual donor satisfaction with communications",
            Math.Round((decimal)avgCommSatisfaction, 1), "%", "85", commRate.Any() ? "Active" : "Planning",
            commRate.Any()
                ? $"{commRate.Count} communication entries, {avgCommSatisfaction:F1}% average satisfaction | Form: Data Entry → Communication Rate"
                : "No communication rate data yet - Go to Data Entry → Communication Rate", nextId++, metricType: "Percentage", fiscalYear: fiscalYear,
            sampleCount: commRate.Count,
            sampleCountText: commRate.Any() ? $"{commRate.Count} communication entr{(commRate.Count == 1 ? "y" : "ies")} averaged" : string.Empty);
    }

    private async Task AddCommunityMetricsAsync(StrategicGoal goal, string fiscalYear)
    {
        var nextId = goal.Metrics.Count + 4000;

        var interfaithRows = FilterByFiscalYearDate(await _context.Interfaith_11D.ToListAsync(), fiscalYear, i => i.CreatedDate);
        AddOrUpdateMetric(goal, "Interfaith Events Hosted", "Interfaith collaborative events logged",
            interfaithRows.Count, "events", "12", interfaithRows.Any() ? "Active" : "Planning",
            interfaithRows.Any()
                ? $"{interfaithRows.Count} interfaith event entries submitted | Form: Data Entry → Interfaith 11D"
                : "No interfaith event entries yet - Go to Data Entry → Interfaith 11D", nextId++, metricType: "Count", fiscalYear: fiscalYear);

        var youthRows = FilterByFiscalYearDate(
            await _context.YouthAttend_15D
                .OrderByDescending(y => y.CreatedDate)
                .ToListAsync(),
            fiscalYear,
            y => y.CreatedDate)
            .OrderByDescending(y => y.CreatedDate)
            .ToList();
        var youthGrowth = 0m;
        var hasYouthGrowth = false;
        if (youthRows.Count >= 2)
        {
            var latestYouth = youthRows[0];
            var previousYouth = youthRows[1];
            if (previousYouth.NumberOfYouthAttendees > 0)
            {
                youthGrowth = Math.Round((decimal)((double)(latestYouth.NumberOfYouthAttendees - previousYouth.NumberOfYouthAttendees)
                    / previousYouth.NumberOfYouthAttendees * 100), 1);
                hasYouthGrowth = true;
            }
        }

        AddOrUpdateMetric(goal, "Youth Attendance Growth", "Growth between the two most recent youth attendance entries",
            youthGrowth, "%", "20", hasYouthGrowth ? "Active" : "Planning",
            hasYouthGrowth
                ? $"{youthGrowth:F1}% growth based on the two most recent youth attendance records | Form: Data Entry → CommYouth15D"
                : "Need at least two youth attendance entries to calculate growth - Go to Data Entry → CommYouth15D", nextId++, metricType: "Percentage", fiscalYear: fiscalYear);

        var partnerRows = (await _context.CollabTouch_47D.ToListAsync())
            .Where(p => FiscalYearMatches(p.FiscalYear, fiscalYear))
            .ToList();
        AddOrUpdateMetric(goal, "Cross-Sector Collaborations", "Collaborative partner touchpoints logged",
            partnerRows.Count, "partners", "10", partnerRows.Any() ? "Active" : "Planning",
            partnerRows.Any()
                ? $"{partnerRows.Count} collaborative partner touchpoints logged | Form: Data Entry → CommCollab47D"
                : "No collaborative partner touchpoints yet - Go to Data Entry → CommCollab47D", nextId++, metricType: "Count", fiscalYear: fiscalYear);

        var faithRows = FilterByFiscalYearDate(await _context.FaithCommunity_13D.ToListAsync(), fiscalYear, f => f.CreatedDate);
        var faithGoalPercent = 0m;
        if (faithRows.Any())
        {
            faithGoalPercent = Math.Round((decimal)faithRows.Count(f => f.NumberOfFaithsRepresented >= 3) / faithRows.Count * 100, 1);
        }
        AddOrUpdateMetric(goal, "Faith Representation", "Percent of community events with three or more faiths represented",
            faithGoalPercent, "%", "80", faithRows.Any() ? "Active" : "Planning",
            faithRows.Any()
                ? $"{faithRows.Count(f => f.NumberOfFaithsRepresented >= 3)} of {faithRows.Count} entries met the 3-faith threshold | Form: Data Entry → CommFaith13D"
                : "No faith representation entries yet - Go to Data Entry → CommFaith13D", nextId++, metricType: "Percentage", fiscalYear: fiscalYear);

        var eventSatisfaction = FilterByFiscalYearDate(await _context.EventSatisfaction_12D.ToListAsync(), fiscalYear, e => e.CreatedDate);
        var avgEventSatisfaction = eventSatisfaction.Any() ? Math.Round((decimal)eventSatisfaction.Average(e => e.EventAttendeeSatisfactionPercentage), 1) : 0m;
        AddOrUpdateMetric(goal, "Event Satisfaction", "Community event satisfaction",
            avgEventSatisfaction, "%", "90", eventSatisfaction.Any() ? "Active" : "Planning",
            eventSatisfaction.Any()
                ? $"Average event satisfaction is {avgEventSatisfaction:F1}% across {eventSatisfaction.Count} entries | Form: Data Entry → EventSatisfaction12D"
                : "No event satisfaction entries yet - Go to Data Entry → EventSatisfaction12D", nextId++, metricType: "Percentage", fiscalYear: fiscalYear,
            sampleCount: eventSatisfaction.Count,
            sampleCountText: eventSatisfaction.Any() ? $"{eventSatisfaction.Count} feedback entr{(eventSatisfaction.Count == 1 ? "y" : "ies")} averaged" : string.Empty);

        var contactRows = FilterByFiscalYearYears(
            await _context.ContactsInterfaith_14D
                .OrderByDescending(c => c.Year)
                .ToListAsync(),
            fiscalYear,
            c => c.Year)
            .OrderByDescending(c => c.Year)
            .ToList();
        var contactGrowth = 0m;
        var hasContactGrowth = false;
        if (contactRows.Count >= 2)
        {
            var latestContacts = contactRows[0];
            var previousContacts = contactRows[1];
            if (previousContacts.TotalInterfaithContacts > 0)
            {
                contactGrowth = Math.Round((decimal)((double)(latestContacts.TotalInterfaithContacts - previousContacts.TotalInterfaithContacts)
                    / previousContacts.TotalInterfaithContacts * 100), 1);
                hasContactGrowth = true;
            }
        }
        AddOrUpdateMetric(goal, "Clergy Network Growth", "Growth in clergy and interfaith contacts between the two latest yearly entries",
            contactGrowth, "%", "25", hasContactGrowth ? "Active" : "Planning",
            hasContactGrowth
                ? $"{contactGrowth:F1}% growth between the two most recent interfaith contact entries | Form: Data Entry → CommContact14D"
                : "Need at least two interfaith contact entries to calculate growth - Go to Data Entry → CommContact14D", nextId++, metricType: "Percentage", fiscalYear: fiscalYear);

        var diversityRows = (await _context.Diversity_37D
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync())
            .Where(d => FiscalYearMatches(d.FiscalYear, fiscalYear))
            .OrderByDescending(d => d.CreatedDate)
            .ToList();
        var diversityGrowth = 0m;
        var hasDiversityGrowth = false;
        if (diversityRows.Count >= 2)
        {
            var latestDiversity = diversityRows[0];
            var previousDiversity = diversityRows[1];
            if (previousDiversity.DiversityCount > 0)
            {
                diversityGrowth = Math.Round((decimal)((double)(latestDiversity.DiversityCount - previousDiversity.DiversityCount)
                    / previousDiversity.DiversityCount * 100), 1);
                hasDiversityGrowth = true;
            }
        }
        AddOrUpdateMetric(goal, "Participant Diversity", "Growth in diverse participation between the latest two entries",
            diversityGrowth, "%", "10", hasDiversityGrowth ? "Active" : "Planning",
            hasDiversityGrowth
                ? $"{diversityGrowth:F1}% diversity growth based on the latest two entries | Form: Data Entry → CommDiversity37D"
                : "Need at least two diversity entries to calculate growth - Go to Data Entry → CommDiversity37D", nextId++, metricType: "Percentage", fiscalYear: fiscalYear);

        var firstTimeRows = (await _context.FirstTime_38D.ToListAsync())
            .Where(f => FiscalYearMatches(f.FiscalYear, fiscalYear))
            .ToList();
        var avgFirstTimeRate = firstTimeRows.Any() ? Math.Round((decimal)firstTimeRows.Average(f => f.FirstTimeParticipantRate), 1) : 0m;
        AddOrUpdateMetric(goal, "First-Time Participants", "Average share of first-time participants across logged events",
            avgFirstTimeRate, "%", "25", firstTimeRows.Any() ? "Active" : "Planning",
            firstTimeRows.Any()
                ? $"{avgFirstTimeRate:F1}% average first-time participation across {firstTimeRows.Count} entries | Form: Data Entry → CommFirst38D"
                : "No first-time participant entries yet - Go to Data Entry → CommFirst38D", nextId++, metricType: "Percentage", fiscalYear: fiscalYear);
    }

    private void AddOrUpdateMetric(StrategicGoal goal, string name, string description, decimal currentValue, 
                                 string unit, string target, string status, string detailedDescription, int id, string metricType = "Count", string? fiscalYear = null,
                                 int? sampleCount = null, int? minimumSampleSize = null, string? sampleCountText = null, string? sampleRequirementText = null)
    {
        var existingMetric = goal.Metrics.FirstOrDefault(m => m.Name == name);
        var hasIncomingData = !string.Equals(status, "Planning", StringComparison.OrdinalIgnoreCase);
        var resolvedFiscalYear = string.IsNullOrWhiteSpace(fiscalYear) ? GetCurrentFiscalYearLabel() : fiscalYear;
        
        if (existingMetric != null)
        {
            existingMetric.CurrentValue = currentValue;
            existingMetric.Status = ResolveMetricStatus(existingMetric.Status, currentValue, target, hasIncomingData, sampleCount, minimumSampleSize);
            existingMetric.Description = detailedDescription;
            existingMetric.Target = target;
            existingMetric.Unit = unit;
            existingMetric.MetricType = metricType;
            existingMetric.FiscalYear = resolvedFiscalYear;
            existingMetric.SampleCount = sampleCount;
            existingMetric.MinimumSampleSize = minimumSampleSize;
            existingMetric.SampleCountText = sampleCountText ?? string.Empty;
            existingMetric.SampleRequirementText = sampleRequirementText ?? string.Empty;
        }
        else
        {
            var resolvedStatus = ResolveMetricStatus(status, currentValue, target, hasIncomingData, sampleCount, minimumSampleSize);

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
                MetricType = metricType,
                IsPublic = true,
                FiscalYear = resolvedFiscalYear,
                Status = resolvedStatus,
                TargetDate = DateTime.Now.AddMonths(12),
                SampleCount = sampleCount,
                MinimumSampleSize = minimumSampleSize,
                SampleCountText = sampleCountText ?? string.Empty,
                SampleRequirementText = sampleRequirementText ?? string.Empty
            });
        }
    }

    private string ResolveMetricStatus(string priorStatus, decimal currentValue, string target, bool hasIncomingData, int? sampleCount = null, int? minimumSampleSize = null)
    {
        var hasSampleRequirement = minimumSampleSize.GetValueOrDefault() > 0;
        var hasSufficientSample = !hasSampleRequirement || sampleCount.GetValueOrDefault() >= minimumSampleSize.GetValueOrDefault();

        if (TryParseMetricTarget(target, out var targetValue) && targetValue > 0 && currentValue >= targetValue)
        {
            return hasSufficientSample ? "Completed" : "Building Sample";
        }

        if (currentValue > 0
            || hasIncomingData
            || string.Equals(priorStatus, "Active", StringComparison.OrdinalIgnoreCase)
            || string.Equals(priorStatus, "Completed", StringComparison.OrdinalIgnoreCase))
        {
            return hasSufficientSample ? "Active" : "Building Sample";
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

        return DashboardMetricRules.TryParseTarget(cleaned, out targetValue);
    }

    private ChartData BuildChartData(List<StrategicGoal> goals, string fiscalYear)
    {
        var chartData = new ChartData();

        // Goal Progress Data - Calculate actual progress from metrics
        var orgGoal = goals.FirstOrDefault(g => g.Name.Contains("Organizational"));
        var finGoal = goals.FirstOrDefault(g => g.Name.Contains("Financial"));
        var identityGoal = goals.FirstOrDefault(g => g.Name.Contains("Identity"));
        var communityGoal = goals.FirstOrDefault(g => g.Name.Contains("Community"));

        chartData.GoalProgress = new GoalProgressData
        {
            OrganizationalProgress = CalculateGoalProgress(orgGoal, fiscalYear),
            FinancialProgress = CalculateGoalProgress(finGoal, fiscalYear),
            IdentityProgress = CalculateGoalProgress(identityGoal, fiscalYear),
            CommunityProgress = CalculateGoalProgress(communityGoal, fiscalYear)
        };

        // Monthly Trends - Based on actual data creation dates
        chartData.MonthlyTrends = BuildMonthlyTrends(goals);

        // Metric Types Distribution
        chartData.MetricTypes = BuildMetricTypesData(goals, fiscalYear);

        // Quarterly Data from website traffic
        chartData.QuarterlyData = BuildQuarterlyData(fiscalYear);

        return chartData;
    }

    private decimal CalculateGoalProgress(StrategicGoal? goal, string fiscalYear)
    {
        if (goal?.Metrics == null || !goal.Metrics.Any())
            return 0;

        return DashboardMetricRules.CalculateGoalProgress(goal.Metrics, fiscalYear);
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

    private List<MetricTypeData> BuildMetricTypesData(List<StrategicGoal> goals, string fiscalYear)
    {
        return DashboardMetricRules.BuildMetricDistribution(
            goals.SelectMany(g => g.Metrics ?? Enumerable.Empty<GoalMetric>()),
            fiscalYear).ToList();
    }

    private List<QuarterlyData> BuildQuarterlyData(string fiscalYear)
    {
        var quarterlyData = new List<QuarterlyData>();
        
        try
        {
            var websiteTraffic = FilterByFiscalYearDate(_context.WebsiteTraffic.ToList(), fiscalYear, w => w.CreatedDate);
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
