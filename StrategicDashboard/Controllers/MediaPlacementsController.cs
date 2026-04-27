using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using OneJaxDashboard.Services;
//karrie's
namespace OneJaxDashboard.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class MediaPlacementsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;
        private readonly ILogger<MediaPlacementsController> _logger;
        private readonly SqlServerInsertCompatibilityService _sqlServerInsertCompatibility;

        public MediaPlacementsController(
            ApplicationDbContext context,
            ActivityLogService activityLog,
            ILogger<MediaPlacementsController> logger,
            SqlServerInsertCompatibilityService sqlServerInsertCompatibility)
        {
            _context = context;
            _activityLog = activityLog;
            _logger = logger;
            _sqlServerInsertCompatibility = sqlServerInsertCompatibility;
        }

        // GET: MediaPlacements/Index
        [HttpGet]
        public IActionResult Index()
        {
            TryLoadStats();
            return View(new MediaPlacements_3D());
        }

        // POST: MediaPlacements/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(MediaPlacements_3D model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _sqlServerInsertCompatibility.PrepareForInsert(model, "MediaPlacements_3D");
                    _context.MediaPlacements_3D.Add(model);
                    _context.SaveChanges();

                    var actor = User.Identity?.Name ?? "Unknown";
                    _activityLog.Log(actor, "Submitted Media Placements Record", "MediaPlacements",
                        details: $"Id={model.Id}; Total mentions: {model.TotalMentions}");
                    
                    // Recalculate grand total after adding new entry
                    var allEntries = _context.MediaPlacements_3D.ToList();
                    var grandTotal = allEntries.Sum(e => e.TotalMentions);
                    
                    ViewBag.GrandTotal = grandTotal;
                    ViewBag.TotalEntries = allEntries.Count;
                    
                    TempData["Success"] = "Media placements record submitted successfully!";
                    ViewBag.ShowNewEntryButton = true;
                    return View(model); // Show the submitted data instead of redirecting
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save Media Placements record.");
                    TempData["Error"] = $"Error saving record: {ex.GetBaseException().Message}";
                }
            }

            TryLoadStats();
            return View(model);
        }

        private void LoadStats()
        {
            var allEntries = _context.MediaPlacements_3D.ToList();
            ViewBag.GrandTotal = allEntries.Sum(e => e.TotalMentions);
            ViewBag.TotalEntries = allEntries.Count;
        }

        private void TryLoadStats()
        {
            try
            {
                LoadStats();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load Media Placements form data.");
                TempData["Error"] ??= BuildLoadErrorMessage("Media Placements", ex);
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
