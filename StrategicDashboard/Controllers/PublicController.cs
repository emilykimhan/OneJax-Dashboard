using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneJax.StrategicDashboard.Models;
using OneJaxDashboard.Data;

namespace StrategicDashboard.Controllers
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
                            .Include(g => g.Events)
                            .ToList();
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
                            .Include(g => g.Events)
                            .FirstOrDefault(g => g.Id == id);
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
                            Date = DateTime.Now.AddDays(30),
                            Status = "Planned",
                            StrategicGoalId = 1,
                            Type = "Workshop",
                            Location = "OneJax Center"
                        },
                        new Event
                        {
                            Id = 2,
                            Title = "Youth Development Program",
                            Date = DateTime.Now.AddDays(-15),
                            Status = "Active",
                            StrategicGoalId = 1,
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
                            Date = DateTime.Now.AddDays(-60),
                            Status = "Completed",
                            StrategicGoalId = 2,
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
                            Date = DateTime.Now.AddDays(45),
                            Status = "Planned",
                            StrategicGoalId = 3,
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
                            Date = DateTime.Now.AddDays(-5),
                            Status = "Completed",
                            StrategicGoalId = 4,
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
    }
}
