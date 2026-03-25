using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using OneJax.StrategicDashboard.Models;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
//emily's
namespace OneJaxDashboard.Controllers
{
    public class PublicController : Controller
    {
        private readonly ApplicationDbContext _context;
        public PublicController(ApplicationDbContext context)
        {
            _context = context;
        }

        // User Story 1: Quick overview of all projects
        public IActionResult Overview()
        {
            try
            {
                // Get all strategic goals with their metrics and events
                var goals = new List<StrategicGoal>();
                
                try
                {
                    if (_context.StrategicGoals != null)
                    {
                        goals = _context.StrategicGoals
                            .Include(g => g.Metrics)
                            .ToList();
                        AttachEventsToGoals(goals);
                    }
                }
                catch
                {
                    // If database tables don't exist, use sample data
                    goals = GetSampleGoals();
                }

                if (!goals.Any())
                {
                    goals = GetSampleGoals();
                }

                return View(goals);
            }
            catch
            {
                // Fallback to sample data
                return View(GetSampleGoals());
            }
        }

        // Read-only public events listing (does not allow creating/editing events).
        [AllowAnonymous]
        public IActionResult Events(int? goalId)
        {
            var query = _context.Strategies
                .Where(s => !s.IsArchived);

            if (goalId.HasValue)
            {
                query = query.Where(s => s.StrategicGoalId == goalId.Value);
            }

            var items = query
                .OrderBy(s => s.StrategicGoalId)
                .ThenBy(s => s.Date)
                .ThenBy(s => s.Time)
                .ToList();

            ViewBag.GoalId = goalId;
            ViewBag.Goals = _context.StrategicGoals
                .Where(g => g.Id >= 1 && g.Id <= 4)
                .OrderBy(g => g.Id)
                .ToList();

            return View(items);
        }

        // User Story 2: Status of a specific project
        public IActionResult Project(int id)
        {
            try
            {
                StrategicGoal? goal = null;
                
                try
                {
                    if (_context.StrategicGoals != null)
                    {
                        goal = _context.StrategicGoals
                            .Include(g => g.Metrics)
                            .FirstOrDefault(g => g.Id == id);
                        if (goal != null)
                        {
                            AttachEventsToGoals(new List<StrategicGoal> { goal });
                        }
                    }
                }
                catch
                {
                    // Database not available, use sample data
                }

                if (goal == null)
                {
                    // Try to get from sample data
                    var sampleGoals = GetSampleGoals();
                    goal = sampleGoals.FirstOrDefault(g => g.Id == id);
                }

                if (goal == null)
                {
                    return NotFound("Project not found");
                }

                return View(goal);
            }
            catch
            {
                return NotFound("Unable to load project details");
            }
        }

        private List<StrategicGoal> GetSampleGoals()
        {
            return new List<StrategicGoal>
            {
                new StrategicGoal
                {
                    Id = 1,
                    Name = "Community Engagement",
                    Description = "Building partnerships and community connections",
                    Color = "#3B82F6",
                    Events = new List<Event>
                    {
                        new Event
                        {
                            Id = 1,
                            Title = "Community Partnership Forum",
                            DueDate = DateTime.Now.AddDays(30),
                            Status = "Planned",
                            Type = "Workshop",
                            Location = "OneJax Center"
                        },
                        new Event
                        {
                            Id = 2,
                            Title = "Youth Development Program",
                            DueDate = DateTime.Now.AddDays(-15),
                            Status = "Active",
                            Type = "Program",
                            Location = "Various Locations"
                        }
                    },
                    Metrics = new List<GoalMetric>
                    {
                        new GoalMetric
                        {
                            Id = 1,
                            Name = "Community Events Hosted",
                            Description = "Number of community engagement events hosted this year",
                            StrategicGoalId = 1,
                            Target = "12",
                            CurrentValue = 8,
                            Unit = "events",
                            Status = "Active",
                            TargetDate = DateTime.Now.AddMonths(3)
                        }
                    }
                },
                new StrategicGoal
                {
                    Id = 2,
                    Name = "Identity/Value Proposition",
                    Description = "Establishing and communicating OneJax's unique identity and value",
                    Color = "#F59E0B",
                    Events = new List<Event>
                    {
                        new Event
                        {
                            Id = 3,
                            Title = "Brand Identity Launch",
                            DueDate = DateTime.Now.AddDays(-60),
                            Status = "Completed",
                            Type = "Launch",
                            Location = "Digital Platforms"
                        }
                    },
                    Metrics = new List<GoalMetric>
                    {
                        new GoalMetric
                        {
                            Id = 2,
                            Name = "Media Placements",
                            Description = "Earned media placements achieved",
                            StrategicGoalId = 2,
                            Target = "12",
                            CurrentValue = 5,
                            Unit = "placements",
                            Status = "Active",
                            TargetDate = DateTime.Now.AddMonths(6)
                        }
                    }
                },
                new StrategicGoal
                {
                    Id = 3,
                    Name = "Financial Stability",
                    Description = "Ensuring sustainable financial health and growth",
                    Color = "#10B981",
                    Events = new List<Event>
                    {
                        new Event
                        {
                            Id = 4,
                            Title = "Fundraising Gala",
                            DueDate = DateTime.Now.AddDays(45),
                            Status = "Planned",
                            Type = "Fundraising",
                            Location = "Downtown Convention Center"
                        }
                    },
                    Metrics = new List<GoalMetric>
                    {
                        new GoalMetric
                        {
                            Id = 3,
                            Name = "Revenue Growth",
                            Description = "Annual revenue growth percentage",
                            StrategicGoalId = 3,
                            Target = "15",
                            CurrentValue = 12,
                            Unit = "%",
                            Status = "Active",
                            TargetDate = DateTime.Now.AddMonths(8)
                        }
                    }
                },
                new StrategicGoal
                {
                    Id = 4,
                    Name = "Organizational Building",
                    Description = "Strengthening organizational structure and capacity",
                    Color = "#1E3A8A",
                    Events = new List<Event>
                    {
                        new Event
                        {
                            Id = 5,
                            Title = "Staff Development Workshop",
                            DueDate = DateTime.Now.AddDays(-5),
                            Status = "Completed",
                            Type = "Training",
                            Location = "OneJax Office"
                        }
                    },
                    Metrics = new List<GoalMetric>
                    {
                        new GoalMetric
                        {
                            Id = 4,
                            Name = "Staff Satisfaction",
                            Description = "Average staff satisfaction rating",
                            StrategicGoalId = 4,
                            Target = "85",
                            CurrentValue = 78,
                            Unit = "%",
                            Status = "Active",
                            TargetDate = DateTime.Now.AddMonths(4)
                        }
                    }
                }
            };
        }

        private void AttachEventsToGoals(List<StrategicGoal> goals)
        {
            if (_context.Events == null || _context.Strategies == null || goals.Count == 0)
            {
                return;
            }

            var strategyLookup = _context.Strategies
                .Select(s => new { s.Id, s.StrategicGoalId })
                .ToList();

            var events = _context.Events
                .Where(e => !e.IsArchived && e.StrategyId.HasValue)
                .ToList();

            var eventsByGoalId = new Dictionary<int, List<Event>>();

            foreach (var evt in events)
            {
                var strategyId = evt.StrategyId;
                if (!strategyId.HasValue)
                {
                    continue;
                }

                var goalId = strategyLookup
                    .Where(s => s.Id == strategyId.Value)
                    .Select(s => (int?)s.StrategicGoalId)
                    .FirstOrDefault();

                if (!goalId.HasValue)
                {
                    continue;
                }

                if (!eventsByGoalId.TryGetValue(goalId.Value, out var list))
                {
                    list = new List<Event>();
                    eventsByGoalId[goalId.Value] = list;
                }

                list.Add(evt);
            }

            foreach (var goal in goals)
            {
                if (eventsByGoalId.TryGetValue(goal.Id, out var list))
                {
                    goal.Events = list;
                }
                else
                {
                    goal.Events = new List<Event>();
                }
            }
        }
    }
}
