using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using OneJaxDashboard.Services;
//Karrie's
namespace OneJaxDashboard.Controllers
{
    public class WebsiteTrafficController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;

        public WebsiteTrafficController(ApplicationDbContext context, ActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }

        // GET: WebsiteTraffic/Index
        [HttpGet]
        public IActionResult Index()
        {
            // Calculate grand total across all entries
            var allEntries = _context.WebsiteTraffic.ToList();
            var grandTotal = allEntries.Sum(e => e.TotalClicks);
            
            ViewBag.GrandTotal = grandTotal;
            ViewBag.TotalEntries = allEntries.Count;
            
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
                    
                    // Log the activity
                    var username = User.Identity?.Name ?? "Unknown";
                    _activityLog.Log(username, "Created Website Traffic", "WebsiteTraffic_4D", model.Id, $"Total Clicks: {model.TotalClicks}");
                    
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
                    TempData["Error"] = $"Error saving record: {ex.Message}";
                }
            }

            return View(model);
        }
    }
}
