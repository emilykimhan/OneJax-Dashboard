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

    // FIXED 3-YEAR STRATEGIC PLAN - These goals with sample events
    // Clean slate - all data will come from database
    private static readonly List<StrategicGoal> _threeYearPlan = new List<StrategicGoal>();

    public IActionResult Index(string status, string time, string goal, string fiscalYear, string quarter)
    {
        try 
        {
            // Try to get data from database first
            var dbGoals = new List<StrategicGoal>();
            
            // Check if StrategicGoals table exists and has data
            try 
            {
                if (_context.StrategicGoals != null)
                {
                    dbGoals = _context.StrategicGoals
                        .Include(g => g.Metrics)
                        .Include(g => g.Events)
                        .ToList();
                }
            }
            catch 
            {
                // Table doesn't exist yet, fall back to hardcoded data
                dbGoals = new List<StrategicGoal>();
            }

            // If no database data, try to generate from survey data
            List<StrategicGoal> allGoals;
            if (dbGoals.Any())
            {
                allGoals = dbGoals;
                ViewBag.DataSource = "Database";
            }
            else
            {
                // Try to generate goals from survey and professional development data
                var generatedGoals = GenerateGoalsFromSurveyData();
                if (generatedGoals.Any())
                {
                    allGoals = generatedGoals;
                    ViewBag.DataSource = "Generated from Survey Data";
                    ViewBag.Message = "Displaying metrics generated from your submitted surveys and professional development data.";
                }
                else
                {
                    // Fall back to hardcoded data
                    allGoals = GetHardcodedGoals();
                    ViewBag.DataSource = "Hardcoded (sample data)";
                    ViewBag.Message = "Using sample data. Submit surveys through Data Entry to see real metrics.";
                }
            }

            // Filter goals if specific goal is requested
            if (!string.IsNullOrEmpty(goal))
            {
                allGoals = allGoals.Where(g => g.Name == goal).ToList();
            }

            // Filter events by status and time period within each goal
            foreach (var g in allGoals)
            {
                if (g.Events != null)
                {
                    g.Events = g.Events
                        .Where(e => (string.IsNullOrEmpty(status) || e.Status == status))
                        .Where(e => FilterByTimePeriod(e, time, fiscalYear, quarter))
                        .ToList();
                }

                if (g.Metrics != null)
                {
                    g.Metrics = g.Metrics
                        .Where(m => FilterMetricByTimePeriod(m, time, fiscalYear, quarter))
                        .ToList();
                }
            }

            return View(new DashboardViewModel { StrategicGoals = allGoals });
        }
        catch (Exception ex)
        {
            // Handle any errors gracefully by falling back to hardcoded data
            ViewBag.Error = $"Error accessing database: {ex.Message}. Showing sample data.";
            ViewBag.DataSource = "Hardcoded (fallback)";
            return View(new DashboardViewModel { StrategicGoals = GetHardcodedGoals() });
        }
    }

    private List<StrategicGoal> GenerateGoalsFromSurveyData()
    {
        var goals = new List<StrategicGoal>();

        try
        {
            // Get staff survey data
            var staffSurveys = _context.StaffSurveys_22D.ToList();
            var profDev = _context.ProfessionalDevelopments.ToList();

            // Always create the four main strategic goals as tabs
            var orgGoal = new StrategicGoal
            {
                Id = 1,
                Name = "Organizational Building",
                Description = "Staff development and organizational capacity",
                Color = "var(--onejax-navy)",
                Events = new List<Event>(),
                Metrics = new List<GoalMetric>()
            };

            // Generate metrics from staff surveys if available
            if (staffSurveys.Any())
            {
                var avgSatisfaction = staffSurveys.Average(s => s.SatisfactionRate);
                var totalStaff = staffSurveys.Count;
                
                orgGoal.Metrics.Add(new GoalMetric
                {
                    Id = 1,
                    Name = "Staff Satisfaction Rate",
                    Description = $"Based on {totalStaff} staff survey responses",
                    StrategicGoalId = 1,
                    Target = "85",
                    CurrentValue = (decimal)Math.Round(avgSatisfaction, 1),
                    Unit = "%",
                    Status = "Active",
                    TargetDate = DateTime.Now.AddMonths(6)
                });

                var totalProfDevFromSurveys = staffSurveys.Sum(s => s.ProfessionalDevelopmentCount);
                
                orgGoal.Metrics.Add(new GoalMetric
                {
                    Id = 2,
                    Name = "Professional Development Activities (Staff Survey)",
                    Description = $"Activities reported by staff members",
                    StrategicGoalId = 1,
                    Target = "50",
                    CurrentValue = totalProfDevFromSurveys,
                    Unit = "activities",
                    Status = "Active",
                    TargetDate = DateTime.Now.AddMonths(6)
                });
            }

            // Generate metrics from professional development data if available
            if (profDev.Any())
            {
                var totalDev26 = profDev.Sum(p => p.ProfessionalDevelopmentYear26);
                var totalDev27 = profDev.Sum(p => p.ProfessionalDevelopmentYear27);
                
                orgGoal.Metrics.Add(new GoalMetric
                {
                    Id = 3,
                    Name = "Professional Development Planning",
                    Description = $"Planned activities for 2026-2027",
                    StrategicGoalId = 1,
                    Target = "100",
                    CurrentValue = totalDev26 + totalDev27,
                    Unit = "activities",
                    Status = "Active",
                    TargetDate = DateTime.Now.AddMonths(12),
                    Q1Value = totalDev26,
                    Q2Value = totalDev27
                });
            }

            // Note: Staff surveys generate metrics above, but not events
            // Survey completion is data collection, not an organizational event

            if (profDev.Any())
            {
                orgGoal.Events.Add(new Event
                {
                    Id = 2,
                    Title = $"Professional Development Plans Submitted",
                    Type = "Planning",
                    Notes = $"{profDev.Count} development plans for 2026-2027",
                    DueDate = DateTime.Now.AddDays(-2),
                    Status = "Completed",
                    StrategicGoalId = 1,
                    Attendees = profDev.Count
                });
            }

            goals.Add(orgGoal);

            // Always add the other three goal tabs (even if empty)
            goals.Add(new StrategicGoal
            {
                Id = 2,
                Name = "Identity/Value Proposition",
                Description = "Establishing and communicating OneJax's unique identity and value",
                Color = "var(--onejax-orange)",
                Events = new List<Event>(),
                Metrics = new List<GoalMetric>()
            });

            goals.Add(new StrategicGoal
            {
                Id = 3,
                Name = "Community Engagement",
                Description = "Building partnerships and community connections",
                Color = "var(--onejax-blue)",
                Events = new List<Event>(),
                Metrics = new List<GoalMetric>()
            });

            goals.Add(new StrategicGoal
            {
                Id = 4,
                Name = "Financial Sustainability",
                Description = "Ensuring sustainable financial operations",
                Color = "var(--onejax-green)",
                Events = new List<Event>(),
                Metrics = new List<GoalMetric>()
            });
        }
        catch
        {
            // If survey data can't be processed, return empty goal structure
            goals = new List<StrategicGoal>
            {
                new StrategicGoal
                {
                    Id = 1,
                    Name = "Organizational Building",
                    Description = "Staff development and organizational capacity",
                    Color = "var(--onejax-navy)",
                    Events = new List<Event>(),
                    Metrics = new List<GoalMetric>()
                },
                new StrategicGoal
                {
                    Id = 2,
                    Name = "Identity/Value Proposition",
                    Description = "Establishing and communicating OneJax's unique identity and value",
                    Color = "var(--onejax-orange)",
                    Events = new List<Event>(),
                    Metrics = new List<GoalMetric>()
                },
                new StrategicGoal
                {
                    Id = 3,
                    Name = "Community Engagement",
                    Description = "Building partnerships and community connections",
                    Color = "var(--onejax-blue)",
                    Events = new List<Event>(),
                    Metrics = new List<GoalMetric>()
                },
                new StrategicGoal
                {
                    Id = 4,
                    Name = "Financial Sustainability",
                    Description = "Ensuring sustainable financial operations",
                    Color = "var(--onejax-green)",
                    Events = new List<Event>(),
                    Metrics = new List<GoalMetric>()
                }
            };
        }

        return goals;
    }

    private List<StrategicGoal> GetHardcodedGoals()
    {
        // Return empty strategic goal structure to show the four main tabs
        return new List<StrategicGoal>
        {
            new StrategicGoal
            {
                Id = 1,
                Name = "Community Engagement",
                Description = "Building partnerships and community connections",
                Color = "var(--onejax-blue)",
                Events = new List<Event>(),
                Metrics = new List<GoalMetric>()
            },
            new StrategicGoal
            {
                Id = 2,
                Name = "Identity/Value Proposition", 
                Description = "Establishing and communicating OneJax's unique identity and value",
                Color = "var(--onejax-orange)",
                Events = new List<Event>(),
                Metrics = new List<GoalMetric>()
            },
            new StrategicGoal
            {
                Id = 3,
                Name = "Organizational Building",
                Description = "Building robust organizational capacity and sustainable infrastructure",
                Color = "var(--onejax-navy)",
                Events = new List<Event>(),
                Metrics = new List<GoalMetric>()
            },
            new StrategicGoal
            {
                Id = 4,
                Name = "Financial Sustainability",
                Description = "Ensuring sustainable financial operations and donor engagement",
                Color = "var(--onejax-green)",
                Events = new List<Event>(),
                Metrics = new List<GoalMetric>()
            }
        };
    }

    // Static methods for other controllers to access the fixed plan
    public static List<StrategicGoal> GetThreeYearPlan()
    {
        return _threeYearPlan;
    }

    public static Event? GetEvent(int eventId)
    {
        return _threeYearPlan.SelectMany(g => g.Events)
                           .FirstOrDefault(e => e.Id == eventId);
    }

    public static void AddEventToGoal(int goalId, Event eventItem)
    {
        var goal = _threeYearPlan.FirstOrDefault(g => g.Id == goalId);
        if (goal != null)
        {
            eventItem.Id = goal.Events.Any() ? goal.Events.Max(e => e.Id) + 1 : 1;
            eventItem.StrategicGoalId = goalId;
            goal.Events.Add(eventItem);
        }
    }

    public static List<Event> GetEventsByGoal(int goalId)
    {
        var goal = _threeYearPlan.FirstOrDefault(g => g.Id == goalId);
        return goal?.Events ?? new List<Event>();
    }

    // Time filtering helper methods
    private bool FilterByTimePeriod(Event eventItem, string timePeriod, string fiscalYear, string quarter)
    {
        var now = DateTime.Now;
        var eventDate = eventItem.DueDate ?? DateTime.MinValue;

        // Filter by fiscal year if specified
        if (!string.IsNullOrEmpty(fiscalYear))
        {
            var fy = int.Parse(fiscalYear);
            if (!IsInFiscalYear(eventDate, fy)) return false;
        }

        // Filter by quarter if specified
        if (!string.IsNullOrEmpty(quarter))
        {
            var q = int.Parse(quarter.Substring(1)); // Extract number from "Q1", "Q2", etc.
            var currentFY = GetFiscalYear(now);
            var targetFY = string.IsNullOrEmpty(fiscalYear) ? currentFY : int.Parse(fiscalYear);
            if (!IsInFiscalQuarter(eventDate, targetFY, q)) return false;
        }

        // Filter by general time period if specified
        if (!string.IsNullOrEmpty(timePeriod))
        {
            return timePeriod switch
            {
                "Current-Quarter" => IsInCurrentQuarter(eventDate, now),
                "Next-Quarter" => IsInNextQuarter(eventDate, now),
                "Last-6-Months" => eventDate >= now.AddMonths(-6) && eventDate <= now,
                "Next-6-Months" => eventDate >= now && eventDate <= now.AddMonths(6),
                "3-Year-Plan" => eventDate >= now && eventDate <= now.AddYears(3),
                _ => true
            };
        }

        return true;
    }

    private bool FilterMetricByTimePeriod(GoalMetric metric, string timePeriod, string fiscalYear, string quarter)
    {
        var now = DateTime.Now;
        var targetDate = metric.TargetDate;

        // Filter by fiscal year if specified
        if (!string.IsNullOrEmpty(fiscalYear))
        {
            var fy = int.Parse(fiscalYear);
            if (!IsInFiscalYear(targetDate, fy)) return false;
        }

        // Filter by quarter if specified
        if (!string.IsNullOrEmpty(quarter))
        {
            var q = int.Parse(quarter.Substring(1)); // Extract number from "Q1", "Q2", etc.
            var currentFY = GetFiscalYear(now);
            var targetFY = string.IsNullOrEmpty(fiscalYear) ? currentFY : int.Parse(fiscalYear);
            if (!IsInFiscalQuarter(targetDate, targetFY, q)) return false;
        }

        // Filter by general time period if specified
        if (!string.IsNullOrEmpty(timePeriod))
        {
            return timePeriod switch
            {
                "Current-Quarter" => IsInCurrentQuarter(targetDate, now),
                "Next-Quarter" => IsInNextQuarter(targetDate, now),
                "Last-6-Months" => targetDate >= now.AddMonths(-6) && targetDate <= now,
                "Next-6-Months" => targetDate >= now && targetDate <= now.AddMonths(6),
                "3-Year-Plan" => targetDate >= now && targetDate <= now.AddYears(3),
                _ => true
            };
        }

        return true;
    }

    private int GetFiscalYear(DateTime date)
    {
        // Fiscal year starts in July
        return date.Month >= 7 ? date.Year + 1 : date.Year;
    }

    private bool IsInFiscalYear(DateTime date, int fiscalYear)
    {
        var fyStart = new DateTime(fiscalYear - 1, 7, 1);
        var fyEnd = new DateTime(fiscalYear, 6, 30, 23, 59, 59);
        return date >= fyStart && date <= fyEnd;
    }

    private bool IsInCurrentQuarter(DateTime date, DateTime now)
    {
        var currentQuarter = (now.Month - 1) / 3 + 1;
        var dateQuarter = (date.Month - 1) / 3 + 1;
        return date.Year == now.Year && dateQuarter == currentQuarter;
    }

    private bool IsInNextQuarter(DateTime date, DateTime now)
    {
        var nextQuarterStart = now.AddMonths((4 - (now.Month - 1) % 3 - 1) % 3);
        var nextQuarterEnd = nextQuarterStart.AddMonths(3);
        return date >= nextQuarterStart && date < nextQuarterEnd;
    }

    private bool IsInFiscalQuarter(DateTime date, int fiscalYear, int quarter)
    {
        // Fiscal year starts in July
        var fiscalYearStart = new DateTime(fiscalYear - 1, 7, 1);
        var quarterStart = fiscalYearStart.AddMonths((quarter - 1) * 3);
        var quarterEnd = quarterStart.AddMonths(3);
        return date >= quarterStart && date < quarterEnd;
    }
}
