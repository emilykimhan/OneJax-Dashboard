using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Services;
using OneJaxDashboard.Models;
using OneJaxDashboard.Data;
using System.Security.Claims;
//Talijah might delete
namespace OneJaxDashboard.Controllers
{
    [Authorize(Roles = "Staff")]
    public class StaffPortalController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly EventsService _eventsService;
        private readonly ActivityLogService _activityLog;

        public StaffPortalController(ApplicationDbContext db, EventsService eventsService, ActivityLogService activityLog)
        {
            _db = db;
            _eventsService = eventsService;
            _activityLog = activityLog;
        }

        public IActionResult Index()
        {
            var username = User.Identity?.Name ?? string.Empty;
            var events = _eventsService.GetByOwner(username).ToList();
            var recent = _activityLog.GetRecent(username).ToList();
            var staff = _db.Staffauth.FirstOrDefault(s => s.Username == username);

            ViewData["EventCount"] = events.Count;
            ViewData["AssignedEvents"] = events.Count(e => e.IsAssignedByAdmin);
            ViewData["RecentActivities"] = recent;
            ViewData["FullName"] = staff?.Name ?? string.Empty;
            ViewData["Email"] = staff?.Email ?? string.Empty;
            return View();
        }

        [HttpGet]
        public IActionResult Profile()
        {
            var username = User.Identity?.Name ?? string.Empty;
            var staff = _db.Staffauth.FirstOrDefault(s => s.Username == username);
            if (staff == null)
            {
                // Initialize a profile entry if admin hasn't created one yet
                staff = new Staffauth { 
                    Username = username, 
                    Name = string.Empty, 
                    Email = string.Empty, 
                    Password = ""
                };
                _db.Staffauth.Add(staff);
                _db.SaveChanges();
            }
            return View(staff);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Profile(Staffauth model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var username = User.Identity?.Name ?? string.Empty;
            var staff = _db.Staffauth.FirstOrDefault(s => s.Username == username);
            if (staff != null)
            {
                staff.Name = model.Name;
                staff.Email = model.Email;
            
                _db.SaveChanges();
            }
            var staffName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? staff?.Name ?? username;
            _activityLog.Log(staffName, "Updated Profile", "Profile", null, notes: $"Name={model.Name}; Email={model.Email}");
            TempData["SuccessMessage"] = "Profile updated.";
            return RedirectToAction("Profile");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var username = User.Identity?.Name ?? string.Empty;
            var staff = _db.Staffauth.FirstOrDefault(s => s.Username == username);

            if (staff == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return View(model);
            }

            // Verify current password
            if (staff.Password != model.CurrentPassword)
            {
                ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                return View(model);
            }

            // Update to new password
            staff.Password = model.NewPassword;
            _db.SaveChanges();

            var staffName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? staff?.Name ?? username;
            _activityLog.Log(staffName, "Changed Password", "Security", null, notes: "Password updated successfully");
            
            TempData["SuccessMessage"] = "Password changed successfully.";
            return RedirectToAction("Profile");
        }
    }
}
