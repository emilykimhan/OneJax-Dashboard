using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Models;
using OneJaxDashboard.Data;
//Karrie
namespace OneJaxDashboard.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class DataEntryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DataEntryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RecordHistory()
        {
            return View();
        }

        // Add Strategic Goals
        public IActionResult AddStrategicGoal()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddStrategicGoal(StrategicGoal goal)
        {
            if (ModelState.IsValid)
            {
                _context.StrategicGoals.Add(goal);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Strategic Goal added successfully!";
                return RedirectToAction("ManageGoals");
            }
            return View(goal);
        }

        // Manage Strategic Goals
        public async Task<IActionResult> ManageGoals()
        {
            var goals = await _context.StrategicGoals
                .Include(g => g.Metrics)
                .Include(g => g.Strategies)
                .ToListAsync();
            return View(goals);
        }

        // Add Metrics to Goals
        public async Task<IActionResult> AddMetric(int goalId)
        {
            var goal = await _context.StrategicGoals.FindAsync(goalId);
            if (goal == null)
            {
                return NotFound();
            }
            
            var metric = new GoalMetric
            {
                StrategicGoalId = goalId
            };
            
            ViewBag.GoalName = goal.Name;
            return View(metric);
        }

        [HttpPost]
        public async Task<IActionResult> AddMetric(GoalMetric metric)
        {
            if (ModelState.IsValid)
            {
                _context.GoalMetrics.Add(metric);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Metric added successfully!";
                return RedirectToAction("ManageGoals");
            }
            
            var goal = await _context.StrategicGoals.FindAsync(metric.StrategicGoalId);
            ViewBag.GoalName = goal?.Name ?? "Unknown Goal";
            return View(metric);
        }

        // Add Events
        public async Task<IActionResult> AddEvent()
        {
            var goals = await _context.StrategicGoals.ToListAsync();
            ViewBag.Goals = goals;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddEvent(Event eventItem)
        {
            if (ModelState.IsValid)
            {
                _context.Events.Add(eventItem);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Event added successfully!";
                return RedirectToAction("ManageEvents");
            }
            
            var goals = await _context.StrategicGoals.ToListAsync();
            ViewBag.Goals = goals;
            return View(eventItem);
        }

        // Manage Events
        public async Task<IActionResult> ManageEvents()
        {
            var events = await _context.Events
                .Include(e => e.StrategicGoal)
                .Include(e => e.Strategy)
                .ToListAsync();
            return View(events);
        }

        // DEMO: Seed sample data to test database connection
        public async Task<IActionResult> SeedSampleData()
        {
            // Check if data already exists
            var existingGoals = await _context.StrategicGoals.CountAsync();
            if (existingGoals > 0)
            {
                TempData["Info"] = "Sample data already exists. Use 'Clear All Data' first if you want to reseed.";
                return RedirectToAction("ManageGoals");
            }

            // Create sample strategic goals
            var goal1 = new StrategicGoal
            {
                Name = "Community Engagement",
                Description = "Build stronger community partnerships and engagement",
                Color = "var(--onejax-blue)"
            };

            var goal2 = new StrategicGoal
            {
                Name = "Financial Sustainability",
                Description = "Achieve long-term financial stability",
                Color = "var(--onejax-green)"
            };

            _context.StrategicGoals.AddRange(goal1, goal2);
            await _context.SaveChangesAsync();

            // Add metrics to goals
            var metric1 = new GoalMetric
            {
                StrategicGoalId = goal1.Id,
                Name = "Community Events Hosted",
                Description = "Number of community events hosted per quarter",
                Target = "12",
                CurrentValue = 3m,
                Unit = "events",
                Status = "Active",
                TargetDate = DateTime.Now.AddMonths(6)
            };

            var metric2 = new GoalMetric
            {
                StrategicGoalId = goal2.Id,
                Name = "Annual Revenue",
                Description = "Total annual revenue target",
                Target = "100000",
                CurrentValue = 25000m,
                Unit = "$",
                Status = "Active",
                TargetDate = DateTime.Now.AddMonths(12)
            };

            _context.GoalMetrics.AddRange(metric1, metric2);

            // Add sample events
            var event1 = new Event
            {
                StrategicGoalId = goal1.Id,
                Title = "Interfaith Community Dinner",
                Type = "Community Event",
                Status = "Completed",
                DueDate = DateTime.Now.AddDays(-7),
                Notes = "Successfully hosted dinner with 80 attendees",
                Attendees = 80
            };

            var event2 = new Event
            {
                StrategicGoalId = goal2.Id,
                Title = "Grant Application Submission",
                Type = "Fundraising",
                Status = "Pending",
                DueDate = DateTime.Now.AddDays(14),
                Notes = "Submitted application for community development grant"
            };

            _context.Events.AddRange(event1, event2);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Sample data added successfully! Check the dashboard to see the results.";
            return RedirectToAction("Index", "Home");
        }

        // Clear all dashboard data (for testing)
        public async Task<IActionResult> ClearAllData()
        {
            _context.Events.RemoveRange(_context.Events);
            _context.GoalMetrics.RemoveRange(_context.GoalMetrics);
            _context.Strategies.RemoveRange(_context.Strategies);
            _context.StrategicGoals.RemoveRange(_context.StrategicGoals);
            
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "All dashboard data cleared successfully!";
            return RedirectToAction("Index");
        }

        // Complete the 4-goal structure by adding missing goals
        public async Task<IActionResult> Complete4Goals()
        {
            var existingGoals = await _context.StrategicGoals.ToListAsync();
            var goalNames = existingGoals.Select(g => g.Name).ToList();

            // Add missing strategic goals to complete the 4-goal structure
            var goalsToAdd = new List<StrategicGoal>();

            if (!goalNames.Contains("Community Engagement"))
            {
                goalsToAdd.Add(new StrategicGoal
                {
                    Name = "Community Engagement",
                    Description = "Building partnerships and community connections",
                    Color = "var(--onejax-blue)"
                });
            }

            if (!goalNames.Contains("Identity/Value Proposition"))
            {
                goalsToAdd.Add(new StrategicGoal
                {
                    Name = "Identity/Value Proposition",
                    Description = "Establishing and communicating OneJax's unique identity and value",
                    Color = "var(--onejax-orange)"
                });
            }

            if (!goalNames.Contains("Organizational Building"))
            {
                goalsToAdd.Add(new StrategicGoal
                {
                    Name = "Organizational Building",
                    Description = "Building robust organizational capacity and sustainable infrastructure",
                    Color = "var(--onejax-navy)"
                });
            }

            if (!goalNames.Contains("Financial Sustainability"))
            {
                goalsToAdd.Add(new StrategicGoal
                {
                    Name = "Financial Sustainability",
                    Description = "Ensuring sustainable financial operations and donor engagement",
                    Color = "var(--onejax-green)"
                });
            }

            if (goalsToAdd.Any())
            {
                _context.StrategicGoals.AddRange(goalsToAdd);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Added {goalsToAdd.Count} missing strategic goals. You now have all 4 main tabs!";
            }
            else
            {
                TempData["Info"] = "All 4 strategic goals already exist.";
            }

            return RedirectToAction("Index", "Home");
        }
    }
}