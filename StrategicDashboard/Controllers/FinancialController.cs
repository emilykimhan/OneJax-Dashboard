using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using OneJaxDashboard.Services;

namespace OneJaxDashboard.Controllers
{
    public class FinancialController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;

        public FinancialController(ApplicationDbContext context, ActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new Financial());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(Financial model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Financials.Add(model);
                    _context.SaveChanges();
                    
                    // Log the activity
                    var username = User.Identity?.Name ?? "Unknown";
                    _activityLog.Log(username, "Created Financial Record", "Financial", model.Id, 
                        notes: $"Added financial record");
                    
                    TempData["Success"] = "Financial record submitted successfully!";
                    return RedirectToAction(nameof(Index));
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
