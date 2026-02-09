using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using OneJaxDashboard.Services;

namespace OneJaxDashboard.Controllers
{
    public class YouthAttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;

        public YouthAttendanceController(ApplicationDbContext context, ActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new YouthAttendance());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(YouthAttendance model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.YouthAttendances.Add(model);
                    _context.SaveChanges();
                    
                    // Log the activity
                    var username = User.Identity?.Name ?? "Unknown";
                    _activityLog.Log(username, "Created Youth Attendance", "YouthAttendance", model.Id, 
                        notes: $"Added youth attendance record");
                    
                    TempData["Success"] = "Youth attendance record submitted successfully!";
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
