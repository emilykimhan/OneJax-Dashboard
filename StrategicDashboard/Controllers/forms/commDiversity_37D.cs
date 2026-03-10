using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using OneJaxDashboard.Services;
//Karrie's
namespace OneJaxDashboard.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class CommDiversity37DController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;

        public CommDiversity37DController(ApplicationDbContext context, ActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }

        // GET: CommDiversity37D/Index
        [HttpGet]
        public IActionResult Index()
        {
            LoadStrategiesDropdown();
            LoadStats();
            return View(new Diversity_37D());
        }

        // POST: CommDiversity37D/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(Diversity_37D model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.CreatedDate = DateTime.Now;
                    _context.Diversity_37D.Add(model);
                    _context.SaveChanges();
                    var actor = User?.Identity?.Name ?? "Unknown";
                    _activityLog.Log(actor, "Created Diversity Participation Record", "Diversity",
                        details: $"Id={model.Id}");

                    TempData["Success"] = "Diversity record submitted successfully!";
                    ViewBag.ShowNewEntryButton = true;
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error saving record: {ex.Message}";
                }
            }

            LoadStrategiesDropdown(model.StrategyId);
            LoadStats();
            return View(model);
        }

        // ── Helpers ──────────────────────────────────────────────────
        private void LoadStrategiesDropdown(int? selectedId = null)
        {
            ViewBag.Strategies = new SelectList(
                _context.Strategies.OrderBy(s => s.Name),
                "Id", "Name", selectedId);
        }

        private void LoadStats()
        {
            var allEntries = _context.Diversity_37D
                .Include(d => d.Strategy)
                .OrderByDescending(d => d.CreatedDate)
                .ToList();

            ViewBag.TotalEntries = allEntries.Count;

            if (allEntries.Any())
            {
                ViewBag.LatestDiversityCount = allEntries.First().DiversityCount;
                ViewBag.LatestFiscalYear = allEntries.First().FiscalYear;

                // Calculate % diversity growth between the two most recent entries
                if (allEntries.Count >= 2)
                {
                    var latest = allEntries[0];
                    var previous = allEntries[1];

                    if (previous.DiversityCount > 0)
                    {
                        double growth = ((double)(latest.DiversityCount - previous.DiversityCount)
                                        / previous.DiversityCount) * 100;
                        ViewBag.DiversityGrowthPercent = Math.Round(growth, 1);
                    }
                }
            }
        }
    }
}
