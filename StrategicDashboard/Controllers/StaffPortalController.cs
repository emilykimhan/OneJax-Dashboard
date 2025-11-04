using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrategicDashboard.Models;
using StrategicDashboard.Services;
using OneJaxDashboard.Data;

namespace StrategicDashboard.Controllers
{
    [Authorize(Roles = "Staff")]
    public class StaffPortalController : Controller
    {
    private readonly ApplicationDbContext _db;
        private readonly ProjectsService _projectsService;
        private readonly ActivityLogService _activityLog;

        public StaffPortalController(ApplicationDbContext db, ProjectsService projectsService, ActivityLogService activityLog)
        {
            _db = db;
            _projectsService = projectsService;
            _activityLog = activityLog;
        }

        public IActionResult Index()
        {
            var username = User.Identity?.Name ?? string.Empty;
            var projects = _projectsService.GetByOwner(username).ToList();
            var recent = _activityLog.GetRecent(username).ToList();
            var staff = _db.StaffMembers.FirstOrDefault(s => s.Username == username);

            ViewData["ProjectCount"] = projects.Count;
            ViewData["RecentActivities"] = recent;
            ViewData["FullName"] = staff?.FullName ?? string.Empty;
            ViewData["Email"] = staff?.Email ?? string.Empty;
            return View();
        }

        [HttpGet]
        public IActionResult Profile()
        {
            var username = User.Identity?.Name ?? string.Empty;
            var staff = _db.StaffMembers.FirstOrDefault(s => s.Username == username);
            if (staff == null)
            {
                // Initialize a profile entry if admin hasn't created one yet
                staff = new Staff { Username = username, FullName = string.Empty, Email = string.Empty, Password = "" };
                _db.StaffMembers.Add(staff);
                _db.SaveChanges();
            }
            return View(staff);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Profile(Staff model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var username = User.Identity?.Name ?? string.Empty;
            var staff = _db.StaffMembers.FirstOrDefault(s => s.Username == username);
            if (staff != null)
            {
                staff.FullName = model.FullName;
                staff.Email = model.Email;
                _db.SaveChanges();
            }
            _activityLog.Log(username, "Updated Profile", "Profile", null, notes: $"FullName={model.FullName}; Email={model.Email}");
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
            var staff = _db.StaffMembers.FirstOrDefault(s => s.Username == username);

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

            _activityLog.Log(username, "Changed Password", "Security", null, notes: "Password updated successfully");
            TempData["SuccessMessage"] = "Password changed successfully.";
            return RedirectToAction("Profile");
        }
    }
}
