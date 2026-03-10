using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
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

        public IdentityAnnual_7DController(ApplicationDbContext context, ActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }
        [HttpGet]
        public IActionResult Index()
        {
            // Calculate statistics
            var allEntries = _context.Annual_average_7D.ToList();
            ViewBag.TotalEntries = allEntries.Count;
            
            if (allEntries.Any())
            {
                ViewBag.LatestYear = allEntries.Max(e => e.Year);
                ViewBag.AveragePercentage = allEntries.Average(e => e.Percentage);
                ViewBag.GoalMetCount = allEntries.Count(e => e.GoalMet);
            }
            
            return View(new Annual_average_7D());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(Annual_average_7D model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.CreatedDate = DateTime.Now;
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
                    TempData["Error"] = $"Error saving record: {ex.Message}";
                }
            }

            return View(model);
        }
    }
}
