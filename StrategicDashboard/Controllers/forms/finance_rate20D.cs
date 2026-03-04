using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using OneJaxDashboard.Services;
//Karrie's
namespace OneJaxDashboard.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class FinanceRate20DController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;

        public FinanceRate20DController(ApplicationDbContext context, ActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }

        // GET: FinanceRate20D/Index
        [HttpGet]
        public IActionResult Index()
        {
            // Calculate statistics
            var allEntries = _context.CommunicationRate.ToList();
            ViewBag.TotalEntries = allEntries.Count;
            
            if (allEntries.Any())
            {
                ViewBag.LatestYear = allEntries.Max(e => e.Year);
            }
            
            return View(new Comm_rate20D());
        }

        // POST: FinanceRate20D/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(Comm_rate20D model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.CommunicationRate.Add(model);
                    _context.SaveChanges();

                    var actor = User.Identity?.Name ?? "Unknown";
                    var actorRole = User.IsInRole("Admin") ? "Admin" : "Staff";
                    _activityLog.Log(
                        actor,
                        "Submitted Communication Rate Record",
                        "CommunicationRate",
                        model.Id,
                        notes: $"{actorRole} submitted communication rate for year {model.Year} with value {model.AverageCommunicationSatisfaction}."
                    );
                    
                    // Recalculate statistics after adding new entry
                    var allEntries = _context.CommunicationRate.ToList();
                    ViewBag.TotalEntries = allEntries.Count;
                    ViewBag.LatestYear = allEntries.Max(e => e.Year);
                    
                    TempData["Success"] = "Communication satisfaction record submitted successfully!";
                    ViewBag.ShowNewEntryButton = true;
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
