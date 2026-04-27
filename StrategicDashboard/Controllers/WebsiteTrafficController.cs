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
    public class WebsiteTrafficController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;
        private readonly ILogger<WebsiteTrafficController> _logger;

        public WebsiteTrafficController(
            ApplicationDbContext context,
            ActivityLogService activityLog,
            ILogger<WebsiteTrafficController> logger)
        {
            _context = context;
            _activityLog = activityLog;
            _logger = logger;
        }

        // GET: WebsiteTraffic/Index
        [HttpGet]
        public IActionResult Index()
        {
            TryLoadStats();
            return View(new WebsiteTraffic_4D());
        }

        // POST: WebsiteTraffic/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(WebsiteTraffic_4D model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.WebsiteTraffic.Add(model);
                    _context.SaveChanges();

                    var actor = User.Identity?.Name ?? "Unknown";
                    _activityLog.Log(actor, "Submitted Website Traffic Record", "WebsiteTraffic",
                        details: $"Id={model.Id}; Total clicks: {model.TotalClicks}");
                    
                    // Recalculate grand total after adding new entry
                    var allEntries = _context.WebsiteTraffic.ToList();
                    var grandTotal = allEntries.Sum(e => e.TotalClicks);
                    
                    ViewBag.GrandTotal = grandTotal;
                    ViewBag.TotalEntries = allEntries.Count;
                    
                    TempData["Success"] = "Website traffic record submitted successfully!";
                    return View(model);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save Website Traffic record.");
                    TempData["Error"] = $"Error saving record: {ex.Message}";
                }
            }

            TryLoadStats();
            return View(model);
        }

        private void LoadStats()
        {
            var allEntries = _context.WebsiteTraffic.ToList();
            ViewBag.GrandTotal = allEntries.Sum(e => e.TotalClicks);
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
                _logger.LogError(ex, "Failed to load Website Traffic form data.");
                TempData["Error"] ??= BuildLoadErrorMessage("Website Traffic", ex);
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
