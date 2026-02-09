using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using OneJaxDashboard.Services;

namespace OneJaxDashboard.Controllers
{
    public class IdentityController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;

        public IdentityController(ApplicationDbContext context, ActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new Identity());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(Identity model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Identities.Add(model);
                    _context.SaveChanges();
                    
                    // Log the activity
                    var username = User.Identity?.Name ?? "Unknown";
                    _activityLog.Log(username, "Created Identity Record", "Identity", model.Id, 
                        notes: $"Added identity record");
                    
                    TempData["Success"] = "Identity record submitted successfully!";
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
