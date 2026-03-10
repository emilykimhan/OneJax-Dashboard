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
    public class CommYouth15DController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;

        public CommYouth15DController(ApplicationDbContext context, ActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }

        // GET: CommYouth15D/Index
        [HttpGet]
        public IActionResult Index()
        {
            LoadStrategiesDropdown();
            LoadStats();
            return View(new YouthAttend_15D());
        }

        // POST: CommYouth15D/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(YouthAttend_15D model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.CreatedDate = DateTime.Now;
                    _context.YouthAttend_15D.Add(model);
                    _context.SaveChanges();
                    var actor = User?.Identity?.Name ?? "Unknown";
                    _activityLog.Log(actor, "Created Youth Attendance Record", "YouthAttendance",
                        details: $"Id={model.Id}");

                    TempData["Success"] = "Youth attendance record submitted successfully!";
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
            var allEntries = _context.YouthAttend_15D
                .Include(y => y.Strategy)
                .OrderByDescending(y => y.CreatedDate)
                .ToList();

            ViewBag.TotalEvents = allEntries.Count;

            if (allEntries.Any())
            {
                ViewBag.TotalYouthAttendees = allEntries.Sum(y => y.NumberOfYouthAttendees);
                ViewBag.AvgSatisfaction = Math.Round(allEntries.Average(y => (double)y.PostEventSurveySatisfaction), 1);
                ViewBag.AvgPreAssessment = Math.Round(allEntries.Average(y => (double)y.AveragePreAssessment), 1);
                ViewBag.AvgPostAssessment = Math.Round(allEntries.Average(y => (double)y.AveragePostAssessment), 1);

                // Calculate % growth in youth attendance between the two most recent events
                if (allEntries.Count >= 2)
                {
                    var latest = allEntries[0];
                    var previous = allEntries[1];

                    if (previous.NumberOfYouthAttendees > 0)
                    {
                        double growth = ((double)(latest.NumberOfYouthAttendees - previous.NumberOfYouthAttendees)
                                        / previous.NumberOfYouthAttendees) * 100;
                        ViewBag.AttendanceGrowthPercent = Math.Round(growth, 1);
                    }
                }
            }
        }
    }
}
