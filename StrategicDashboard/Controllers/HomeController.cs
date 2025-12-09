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

    public IActionResult Index(string status, string time, string goal)
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

            if (staffSurveys.Any() || profDev.Any())
            {
                // Create Organizational Building goal from survey data
                var orgGoal = new StrategicGoal
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

                // Generate metrics from professional development data
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

                // Add some events based on recent submissions
                if (staffSurveys.Any())
                {
                    orgGoal.Events.Add(new Event
                    {
                        Id = 1,
                        Title = $"Staff Survey Completed",
                        Type = "Assessment",
                        Notes = $"{staffSurveys.Count} staff members completed satisfaction surveys",
                        DueDate = DateTime.Now.AddDays(-1),
                        Status = "Completed",
                        StrategicGoalId = 1,
                        Attendees = staffSurveys.Count
                    });
                }

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

                // Add other goal templates
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
                    Name = "Financial Stability",
                    Description = "Ensuring sustainable financial operations",
                    Color = "var(--onejax-green)",
                    Events = new List<Event>(),
                    Metrics = new List<GoalMetric>()
                });
            }
        }
        catch
        {
            // If survey data can't be processed, return empty list
            // This will fall back to hardcoded data
        }

        return goals;
    }

    private List<StrategicGoal> GetHardcodedGoals()
    {
        // Return the original hardcoded data as a fallback
        return _threeYearPlan.Select(g => new StrategicGoal
        {
            Id = g.Id,
            Name = g.Name,
            Description = g.Description,
            Color = g.Color,
            Events = g.Events?.ToList() ?? new List<Event>(),
            Metrics = g.Metrics?.ToList() ?? new List<GoalMetric>()
        }).ToList();
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
}
