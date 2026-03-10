using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using OneJaxDashboard.Services;
//Karrie's
namespace OneJaxDashboard.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class CommFirst38DController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;

        public CommFirst38DController(ApplicationDbContext context, ActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }

        // GET: CommFirst38D/Index
        [HttpGet]
        public IActionResult Index()
        {
            LoadStrategiesDropdown();
            LoadStats();
            return View(new FirstTime_38D());
        }

        // POST: CommFirst38D/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(FirstTime_38D model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.CreatedDate = DateTime.Now;
                    _context.FirstTime_38D.Add(model);
                    _context.SaveChanges();
                    var actor = User?.Identity?.Name ?? "Unknown";
                    _activityLog.Log(actor, "Created First-Time Participant Record", "FirstTimeParticipant",
                        details: $"Id={model.Id}");

                    TempData["Success"] = "First-time participant record submitted successfully!";
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
            var allEntries = _context.FirstTime_38D
                .Include(f => f.Strategy)
                .OrderByDescending(f => f.CreatedDate)
                .ToList();

            ViewBag.TotalEntries = allEntries.Count;

            if (allEntries.Any())
            {
                ViewBag.TotalFirstTimeParticipants = allEntries.Sum(f => f.NumberOfFirstTimeParticipants);
                ViewBag.TotalAttendees = allEntries.Sum(f => f.TotalAttendees);
                ViewBag.AvgFirstTimeRate = Math.Round(allEntries.Average(f => (double)f.FirstTimeParticipantRate), 1);
                ViewBag.EventsMeetingGoal = allEntries.Count(f => f.GoalMet);
                ViewBag.LatestFiscalYear = allEntries.First().FiscalYear;
                ViewBag.LatestFirstTimeRate = allEntries.First().FirstTimeParticipantRate;
            }
        }
    }
}
