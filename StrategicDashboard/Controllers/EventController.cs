using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneJax.StrategicDashboard.Models;
using OneJaxDashboard.Data;
using StrategicDashboard.Models;

namespace StrategicDashboard.Controllers
{
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Event/Details/5
        public IActionResult Details(int id)
        {
            try
            {
                // Try to get event from database first
                var eventFromDb = _context.Events?
                    .Where(e => e.Id == id)
                    .FirstOrDefault();
                
                if (eventFromDb != null)
                {
                    // Get the strategic goal name for ViewBag
                    var strategicGoal = _context.StrategicGoals?
                        .FirstOrDefault(g => g.Id == eventFromDb.StrategicGoalId);
                    ViewBag.GoalName = strategicGoal?.Name ?? "Strategic Goal";
                    
                    return View(eventFromDb);
                }
            }
            catch (Exception ex)
            {
                // Log error but continue with sample data
                Console.WriteLine($"Database error: {ex.Message}");
            }
            
            // Use the same logic as HomeController to get goals with events
            var allGoals = GetGoalsWithEvents();
            var sampleEvent = allGoals
                .SelectMany(g => g.Events)
                .FirstOrDefault(e => e.Id == id);
            
            if (sampleEvent != null)
            {
                // Get goal name from the goals data
                var goalForEvent = allGoals.FirstOrDefault(g => g.Id == sampleEvent.StrategicGoalId);
                ViewBag.GoalName = goalForEvent?.Name ?? "Strategic Goal";
                
                return View(sampleEvent);
            }
            
            // If not found, return a default event
            var defaultEvent = new Event
            {
                Id = id,
                Title = "Event Details",
                DueDate = DateTime.Now,
                Type = "General",
                Location = "TBD",
                StrategicGoalId = 1,
                Status = "Planned",
                Notes = "Event details are being updated.",
                Attendees = 0
            };
            
            ViewBag.GoalName = "Strategic Goal";
            return View(defaultEvent);
        }

        private List<StrategicGoal> GetGoalsWithEvents()
        {
            try 
            {
                // Try to get data from database first
                var dbGoals = new List<StrategicGoal>();
                
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
                if (dbGoals.Any())
                {
                    return dbGoals;
                }
                else
                {
                    // Try to generate goals from survey and professional development data
                    var generatedGoals = GenerateGoalsFromSurveyData();
                    if (generatedGoals.Any())
                    {
                        return generatedGoals;
                    }
                    else
                    {
                        // Fall back to hardcoded data
                        return GetHardcodedGoals();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting goals: {ex.Message}");
                return GetHardcodedGoals();
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
                        Description = "Establishing organizational identity and value",
                        Color = "var(--onejax-orange)",
                        Events = new List<Event>(),
                        Metrics = new List<GoalMetric>()
                    });

                    goals.Add(new StrategicGoal
                    {
                        Id = 3,
                        Name = "Community Engagement",
                        Description = "Building community partnerships",
                        Color = "var(--onejax-blue)",
                        Events = new List<Event>(),
                        Metrics = new List<GoalMetric>()
                    });

                    goals.Add(new StrategicGoal
                    {
                        Id = 4,
                        Name = "Financial Stability",
                        Description = "Ensuring sustainable financial health",
                        Color = "var(--onejax-green)",
                        Events = new List<Event>(),
                        Metrics = new List<GoalMetric>()
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating goals from survey data: {ex.Message}");
            }

            return goals;
        }

        private List<StrategicGoal> GetHardcodedGoals()
        {
            // Return the HomeController's static data
            return HomeController.GetThreeYearPlan().Select(g => new StrategicGoal
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                Color = g.Color,
                Events = g.Events?.ToList() ?? new List<Event>(),
                Metrics = g.Metrics?.ToList() ?? new List<GoalMetric>()
            }).ToList();
        }
    }
}
