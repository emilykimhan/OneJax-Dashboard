using Microsoft.AspNetCore.Mvc;
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
            // Clean slate - all data will come from database
            return new List<StrategicGoal>();
        }
    }
}
