using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Services;
using System.Security.Claims;

namespace OneJaxDashboard.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StaffController : Controller
    {
        private readonly StaffService _service;
        private readonly ApplicationDbContext _db;
        private readonly ActivityLogService _activityLog;
        
        public StaffController(StaffService service, ApplicationDbContext db, ActivityLogService activityLog)
        {
            _service = service;
            _db = db;
            _activityLog = activityLog;
        }

        public IActionResult Index()
        {
            var staff = _db.Staffauth.AsNoTracking().ToList();
            return View(staff);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Staffauth staff)
        {
            if (!ModelState.IsValid) return View(staff);
            
            // Prevent duplicate usernames
            if (!string.IsNullOrEmpty(staff.Username) && _db.Staffauth.Any(s => s.Username == staff.Username))
            {
                ModelState.AddModelError("Username", "Username is already taken");
                return View(staff);
            }
            
            // Persist to database
            _db.Staffauth.Add(staff);
            _db.SaveChanges();

            var adminName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? User.Identity?.Name ?? "Admin";
            _activityLog.Log(adminName, "Created Staff Member", "Staff",
                details: $"Id={staff.Id}; Created staff member '{staff.Name}'");

            return RedirectToAction("Index");
        }

        [HttpGet] // ADD THIS
        public IActionResult Edit(int id)
        {
            var staff = _db.Staffauth.AsNoTracking().FirstOrDefault(s => s.Id == id);
            if (staff == null) return NotFound();
            
            // Add a message to the view that username cannot be changed
            ViewBag.UsernameReadOnly = true;
            
            return View(staff);
        }

        [HttpPost] // This one already has it
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Staffauth staff)
        {
            var existing = _db.Staffauth.FirstOrDefault(s => s.Id == staff.Id);
            if (existing == null) return NotFound();

            // Allow keeping current password if left blank
            if (string.IsNullOrWhiteSpace(staff.Password))
            {
                ModelState.Remove(nameof(Staffauth.Password));
                staff.Password = existing.Password;
            }

            if (!ModelState.IsValid) return View(staff);

            var removingLastAdmin = existing.IsAdmin &&
                !staff.IsAdmin &&
                !_db.Staffauth.Any(s => s.Id != existing.Id && s.IsAdmin);

            if (removingLastAdmin)
            {
                ModelState.AddModelError(nameof(Staffauth.IsAdmin), "At least one administrator account must remain.");
                return View(staff);
            }

            existing.Name = staff.Name;
            // DO NOT update Username - it's used as a foreign key and cannot be changed
            existing.Password = staff.Password;
            existing.Email = staff.Email;
            existing.IsAdmin = staff.IsAdmin;

            _db.SaveChanges();

            var adminName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? User.Identity?.Name ?? "Admin";
            _activityLog.Log(adminName, "Updated Staff Member", "Staff",
                details: $"Id={staff.Id}; Updated staff member '{staff.Name}'");

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var staff = _db.Staffauth.AsNoTracking().FirstOrDefault(s => s.Id == id);
            if (staff == null) return NotFound();
            return View(staff);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var staff = _db.Staffauth.FirstOrDefault(s => s.Id == id);
            if (staff != null)
            {
                var deletingLastAdmin = staff.IsAdmin && !_db.Staffauth.Any(s => s.Id != id && s.IsAdmin);
                if (deletingLastAdmin)
                {
                    ModelState.AddModelError(string.Empty, "At least one administrator account must remain.");
                    return View("Delete", staff);
                }

                _db.Staffauth.Remove(staff);
                _db.SaveChanges();

                var adminName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? User.Identity?.Name ?? "Admin";
                _activityLog.Log(adminName, "Deleted Staff Member", "Staff",
                    details: $"Id={id}; Deleted staff member '{staff.Name}'");
            }
            return RedirectToAction("Index");
        }
    }
}
