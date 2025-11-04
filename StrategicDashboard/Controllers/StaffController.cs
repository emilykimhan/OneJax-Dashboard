using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Data;
using StrategicDashboard.Models;
using Microsoft.EntityFrameworkCore;
using StrategicDashboard.Services;

namespace StrategicDashboard.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StaffController : Controller
    {
        private readonly StaffService _service;
        private readonly ApplicationDbContext _db;
        public StaffController(StaffService service, ApplicationDbContext db)
        {
            _service = service;
            _db = db;
        }

        public IActionResult Index()
        {
            var staff = _db.StaffMembers.AsNoTracking().ToList();
            return View(staff);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public IActionResult Create(Staff staff)
        {
            if (!ModelState.IsValid) return View(staff);
            // Prevent duplicate usernames
            if (_service.GetByUsername(staff.Username) != null)
            {
                ModelState.AddModelError("Username", "Username is already taken");
                return View(staff);
            }
            _service.Add(staff);
            // Persist to database
            _db.StaffMembers.Add(staff);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var staff = _db.StaffMembers.AsNoTracking().FirstOrDefault(s => s.Id == id);
            if (staff == null) return NotFound();
            return View(staff);
        }

        [HttpPost]
        public IActionResult Edit(Staff staff)
        {
            var existing = _db.StaffMembers.FirstOrDefault(s => s.Id == staff.Id);
            if (existing == null) return NotFound();

            // Allow keeping current password if left blank
            if (string.IsNullOrWhiteSpace(staff.Password))
            {
                // avoid validation error on Password
                ModelState.Remove(nameof(Staff.Password));
                staff.Password = existing.Password;
            }

            // Prevent duplicate usernames (exclude current)
            if (_db.StaffMembers.Any(s => s.Username == staff.Username && s.Id != staff.Id))
            {
                ModelState.AddModelError("Username", "Username is already taken");
            }

            if (!ModelState.IsValid) return View(staff);

            existing.Username = staff.Username;
            existing.Password = staff.Password;
            existing.FullName = staff.FullName;
            existing.Email = staff.Email;

            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var staff = _db.StaffMembers.AsNoTracking().FirstOrDefault(s => s.Id == id);
            if (staff == null) return NotFound();
            return View(staff);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var staff = _db.StaffMembers.FirstOrDefault(s => s.Id == id);
            if (staff != null)
            {
                _db.StaffMembers.Remove(staff);
                _db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}