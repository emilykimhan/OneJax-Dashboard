using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Services;
using System.Security.Claims;
//talijah's
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
            _activityLog.Log(adminName, "Created Staff Member", "Staff", staff.Id, notes: $"Created staff member '{staff.Name}'");

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var staff = _db.Staffauth.AsNoTracking().FirstOrDefault(s => s.Id == id);
            if (staff == null) return NotFound();
            return View(staff);
        }

        [HttpPost]
        public IActionResult Edit(Staffauth staff)
        {
            var existing = _db.Staffauth.FirstOrDefault(s => s.Id == staff.Id);
            if (existing == null) return NotFound();

            // Allow keeping current password if left blank
            if (string.IsNullOrWhiteSpace(staff.Password))
            {
                // avoid validation error on Password
                ModelState.Remove(nameof(Staffauth.Password));
                staff.Password = existing.Password;
            }

            // Prevent duplicate usernames (exclude current)
            if (!string.IsNullOrEmpty(staff.Username) && 
                _db.Staffauth.Any(s => s.Username == staff.Username && s.Id != staff.Id))
            {
                ModelState.AddModelError("Username", "Username is already taken");
            }

            if (!ModelState.IsValid) return View(staff);

            existing.Name = staff.Name;
            existing.Username = staff.Username;
            existing.Password = staff.Password;
            existing.Email = staff.Email;
           

            _db.SaveChanges();

            var adminName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? User.Identity?.Name ?? "Admin";
            _activityLog.Log(adminName, "Updated Staff Memeber", "Staff", staff.Id, notes: $"Updated staff member '{staff.Name}'");

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var staff = _db.Staffauth.AsNoTracking().FirstOrDefault(s => s.Id == id);
            if (staff == null) return NotFound();
            return View(staff);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var staff = _db.Staffauth.FirstOrDefault(s => s.Id == id);
            if (staff != null)
            {
                _db.Staffauth.Remove(staff);
                _db.SaveChanges();

                var adminName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? User.Identity?.Name ?? "Admin";
                _activityLog.Log(adminName, "Deleted Staff Member", "Staff", id, notes: $"Deleted staff member '{staff.Name}'");
            }
            return RedirectToAction("Index");
        }
    }
}