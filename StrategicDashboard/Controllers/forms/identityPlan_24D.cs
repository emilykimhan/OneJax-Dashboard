using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using OneJaxDashboard.Services;
//Karrie's
namespace OneJaxDashboard.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class identityPlan_24DController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;
        private readonly ILogger<identityPlan_24DController> _logger;

        public identityPlan_24DController(
            ApplicationDbContext context,
            ActivityLogService activityLog,
            ILogger<identityPlan_24DController> logger)
        {
            _context = context;
            _activityLog = activityLog;
            _logger = logger;
        }

        // GET: identityPlan_24D/Index
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                LoadStats();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load Framework Development Plan form data.");
                TempData["Error"] = BuildLoadErrorMessage("Framework Development Plan", ex);
            }

            return View(new Plan2026_24D());
        }

        // POST: identityPlan_24D/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(Plan2026_24D model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.CreatedDate = DateTime.Now;
                    _context.Plan2026_24D.Add(model);
                    _context.SaveChanges();
                    var actor = User?.Identity?.Name ?? "Unknown";
                    _activityLog.Log(actor, "Created Framework Plan Progress Record", "FrameworkPlan2026",
                        details: $"Id={model.Id}");
                    
                    TempData["Success"] = "Submitted successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save Framework Development Plan record for {Name} ({Year}/{Quarter}).",
                        model.Name, model.Year, model.Quarter);
                    TempData["Error"] = $"Error saving record: {ex.Message}";
                }
            }

            TryLoadStats();
            return View(model);
        }

        private void LoadStats()
        {
            var allEntries = _context.Plan2026_24D.ToList();
            ViewBag.TotalEntries = allEntries.Count;

            if (allEntries.Any())
            {
                ViewBag.LatestYear = allEntries.Max(e => e.Year);
                ViewBag.GoalMetCount = allEntries.Count(e => e.GoalMet);
            }
        }

        private void TryLoadStats()
        {
            try
            {
                LoadStats();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh Framework Development Plan stats.");
                TempData["Error"] ??= BuildLoadErrorMessage("Framework Development Plan", ex);
            }
        }

        private static string BuildLoadErrorMessage(string formName, Exception ex)
        {
            var message = ex.GetBaseException().Message;
            var schemaProblem =
                message.Contains("Invalid object name", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("Invalid column name", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("no such table", StringComparison.OrdinalIgnoreCase);

            return schemaProblem
                ? $"{formName} could not load because the Azure database schema is out of date."
                : $"{formName} could not load right now. Please try again.";
        }
    }
}
