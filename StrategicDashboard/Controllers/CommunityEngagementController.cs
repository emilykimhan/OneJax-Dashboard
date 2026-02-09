using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using OneJaxDashboard.Services;

namespace OneJaxDashboard.Controllers
{
    public class CommunityEngagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;

        public CommunityEngagementController(ApplicationDbContext context, ActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new CommunityEngagement());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(CommunityEngagement model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.CommunityEngagements.Add(model);
                    _context.SaveChanges();
                    
                    // Log the activity
                    var username = User.Identity?.Name ?? "Unknown";
                    _activityLog.Log(username, "Created Community Engagement", "CommunityEngagement", model.Id, 
                        notes: $"Added community engagement record");
                    
                    TempData["Success"] = "Community engagement record submitted successfully!";
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
