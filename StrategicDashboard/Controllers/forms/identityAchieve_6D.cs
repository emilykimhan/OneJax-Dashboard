using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using OneJaxDashboard.Services;

namespace OneJaxDashboard.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class identityAchieve_6DController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;
        private readonly ILogger<identityAchieve_6DController> _logger;
        private readonly SqlServerInsertCompatibilityService _sqlServerInsertCompatibility;

        public identityAchieve_6DController(
            ApplicationDbContext context,
            ActivityLogService activityLog,
            ILogger<identityAchieve_6DController> logger,
            SqlServerInsertCompatibilityService sqlServerInsertCompatibility)
        {
            _context = context;
            _activityLog = activityLog;
            _logger = logger;
            _sqlServerInsertCompatibility = sqlServerInsertCompatibility;
        }

        // GET: identityAchieve_6D/Index
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                LoadStats();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load Milestone Achievement form data.");
                TempData["Error"] = BuildLoadErrorMessage("Milestone Achievement", ex);
            }

            return View(new achieveMile_6D());
        }

        // POST: identityAchieve_6D/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(achieveMile_6D model)
        {
            if (!model.Month.HasValue)
                ModelState.AddModelError("Month", "Please select a month.");

            if (ModelState.IsValid)
            {
                try
                {
                    model.CreatedDate = DateTime.Now;
                    _sqlServerInsertCompatibility.PrepareForInsert(model, "achieveMile_6D");
                    _context.achieveMile_6D.Add(model);
                    _context.SaveChanges();
                    var actor = User?.Identity?.Name ?? "Unknown";
                    _activityLog.Log(actor, "Created Milestone Achievement Record", "MilestoneAchievement",
                        details: $"Id={model.Id}");
                    
                    // Recalculate statistics after adding new entry
                    var allEntries = _context.achieveMile_6D.ToList();
                    ViewBag.TotalEntries = allEntries.Count;
                    ViewBag.AveragePercentage = allEntries.Average(e => e.Percentage);
                    ViewBag.GoalMetCount = allEntries.Count(e => e.GoalMet);
                    ViewBag.SixMonthReviewCount = allEntries.Count(e => e.achievedReview);
                    
                    TempData["Success"] = "Milestone achievement record submitted successfully!";
                    ViewBag.ShowNewEntryButton = true;
                    return View(model);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save Milestone Achievement record for year {Year} month {Month}.",
                        model.Year, model.Month);
                    TempData["Error"] = $"Error saving record: {ex.GetBaseException().Message}";
                }
            }

            TryLoadStats();
            return View(model);
        }

        private void LoadStats()
        {
            var allEntries = _context.achieveMile_6D.ToList();
            ViewBag.TotalEntries = allEntries.Count;

            if (allEntries.Any())
            {
                ViewBag.AveragePercentage = allEntries.Average(e => e.Percentage);
                ViewBag.GoalMetCount = allEntries.Count(e => e.GoalMet);
                ViewBag.SixMonthReviewCount = allEntries.Count(e => e.achievedReview);
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
                _logger.LogError(ex, "Failed to refresh Milestone Achievement stats.");
                TempData["Error"] ??= BuildLoadErrorMessage("Milestone Achievement", ex);
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
