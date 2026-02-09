using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using OneJaxDashboard.Services;

namespace OneJaxDashboard.Controllers
{
    public class DonorEngagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;

        public DonorEngagementController(ApplicationDbContext context, ActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new DonorEngagement());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(DonorEngagement model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.DonorEngagements.Add(model);
                    _context.SaveChanges();
                    
                    // Log the activity
                    var username = User.Identity?.Name ?? "Unknown";
                    _activityLog.Log(username, "Created Donor Engagement", "DonorEngagement", model.Id, 
                        notes: $"Added donor engagement record");
                    
                    TempData["Success"] = "Donor engagement record submitted successfully!";
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
