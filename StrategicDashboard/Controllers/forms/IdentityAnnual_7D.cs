using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using OneJaxDashboard.Services;
//Karrie's
namespace OneJaxDashboard.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class IdentityAnnual_7DController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;
        private readonly ILogger<IdentityAnnual_7DController> _logger;
        private readonly SqlServerInsertCompatibilityService _sqlServerInsertCompatibility;

        public IdentityAnnual_7DController(
            ApplicationDbContext context,
            ActivityLogService activityLog,
            ILogger<IdentityAnnual_7DController> logger,
            SqlServerInsertCompatibilityService sqlServerInsertCompatibility)
        {
            _context = context;
            _activityLog = activityLog;
            _logger = logger;
            _sqlServerInsertCompatibility = sqlServerInsertCompatibility;
        }

        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                LoadStats();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load Community Perception Survey form data.");
                TempData["Error"] = BuildLoadErrorMessage("Community Perception Survey", ex);
            }

            return View(new Annual_average_7D());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(Annual_average_7D model)
        {
            if (!model.Month.HasValue)
                ModelState.AddModelError("Month", "Please select a month.");

            if (ModelState.IsValid)
            {
                try
                {
                    model.CreatedDate = DateTime.Now;
                    _sqlServerInsertCompatibility.PrepareForInsert(model, "Annual_average_7D");
                    _context.Annual_average_7D.Add(model);
                    _context.SaveChanges();
                    var actor = User?.Identity?.Name ?? "Unknown";
                    _activityLog.Log(actor, "Created Identity Annual Average Record", "IdentityAnnualAverage",
                        details: $"Id={model.Id}");
                    
                    // Recalculate statistics after adding new entry
                    var allEntries = _context.Annual_average_7D.ToList();
                    ViewBag.TotalEntries = allEntries.Count;
                    ViewBag.LatestYear = allEntries.Max(e => e.Year);
                    ViewBag.AveragePercentage = allEntries.Average(e => e.Percentage);
                    ViewBag.GoalMetCount = allEntries.Count(e => e.GoalMet);
                    
                    TempData["Success"] = "Submitted successfully!";
                    ViewBag.ShowNewEntryButton = true;
                    return View(model);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save Community Perception Survey record for year {Year} month {Month}.",
                        model.Year, model.Month);
                    TempData["Error"] = $"Error saving record: {ex.GetBaseException().Message}";
                }
            }

            TryLoadStats();
            return View(model);
        }

        private void LoadStats()
        {
            var allEntries = _context.Annual_average_7D.ToList();
            ViewBag.TotalEntries = allEntries.Count;

            if (allEntries.Any())
            {
                ViewBag.LatestYear = allEntries.Max(e => e.Year);
                ViewBag.AveragePercentage = allEntries.Average(e => e.Percentage);
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
                _logger.LogError(ex, "Failed to refresh Community Perception Survey stats.");
                TempData["Error"] ??= BuildLoadErrorMessage("Community Perception Survey", ex);
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
