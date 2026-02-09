using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using OneJaxDashboard.Services;

namespace OneJaxDashboard.Controllers
{
    public class InterfaithEventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;

        public InterfaithEventsController(ApplicationDbContext context, ActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new InterfaithEvent());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(InterfaithEvent model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.InterfaithEvents.Add(model);
                    _context.SaveChanges();
                    
                    // Log the activity
                    var username = User.Identity?.Name ?? "Unknown";
                    _activityLog.Log(username, "Created Interfaith Event", "InterfaithEvent", model.Id, 
                        notes: $"Added interfaith event record");
                    
                    TempData["Success"] = "Interfaith event record submitted successfully!";
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
