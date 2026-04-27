using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using OneJaxDashboard.Services;

namespace OneJaxDashboard.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class identitySocial_5DController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;
        private readonly ILogger<identitySocial_5DController> _logger;

        public identitySocial_5DController(
            ApplicationDbContext context,
            ActivityLogService activityLog,
            ILogger<identitySocial_5DController> logger)
        {
            _context = context;
            _activityLog = activityLog;
            _logger = logger;
        }

        // GET: identitySocial_5D/Index
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                LoadStats();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load Social Media Engagement form data.");
                TempData["Error"] = BuildLoadErrorMessage("Social Media Engagement", ex);
            }

            return View(new socialMedia_5D());
        }

        // POST: identitySocial_5D/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(socialMedia_5D model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.CreatedDate = DateTime.Now;
                    _context.socialMedia_5D.Add(model);
                    _context.SaveChanges();
                    var actor = User?.Identity?.Name ?? "Unknown";
                    _activityLog.Log(actor, "Created Social Media Engagement Record", "SocialMediaEngagement",
                        details: $"Id={model.Id}");
                    
                    TempData["Success"] = "Social media engagement data submitted successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save Social Media Engagement record for year {Year}.", model.Year);
                    TempData["Error"] = $"Error saving record: {ex.Message}";
                }
            }

            TryLoadStats();
            return View(model);
        }

        private void LoadStats()
        {
            var allEntries = _context.socialMedia_5D.ToList();
            ViewBag.TotalEntries = allEntries.Count;

            if (allEntries.Any())
            {
                ViewBag.AverageEngagement = allEntries.Average(e => e.AverageEngagementRate);
                ViewBag.GoalMetCount = allEntries.Count(e => e.GoalMet);
                ViewBag.LatestYear = allEntries.Max(e => e.Year);
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
                _logger.LogError(ex, "Failed to refresh Social Media Engagement stats.");
                TempData["Error"] ??= BuildLoadErrorMessage("Social Media Engagement", ex);
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
