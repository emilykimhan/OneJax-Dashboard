using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
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
            var staff = _db.StaffSurveys_22D.AsNoTracking().ToList();
            return View(staff);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public IActionResult Create(StaffSurvey_22D staff)
        {
            if (!ModelState.IsValid) return View(staff);
            // Prevent duplicate usernames
            if (!string.IsNullOrEmpty(staff.Username) && _db.StaffSurveys_22D.Any(s => s.Username == staff.Username))
            {
                ModelState.AddModelError("Username", "Username is already taken");
                return View(staff);
            }
            // Persist to database
            _db.StaffSurveys_22D.Add(staff);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var staff = _db.StaffSurveys_22D.AsNoTracking().FirstOrDefault(s => s.Id == id);
            if (staff == null) return NotFound();
            return View(staff);
        }

        [HttpPost]
        public IActionResult Edit(StaffSurvey_22D staff)
        {
            var existing = _db.StaffSurveys_22D.FirstOrDefault(s => s.Id == staff.Id);
            if (existing == null) return NotFound();

            // Allow keeping current password if left blank
            if (string.IsNullOrWhiteSpace(staff.Password))
            {
                // avoid validation error on Password
                ModelState.Remove(nameof(StaffSurvey_22D.Password));
                staff.Password = existing.Password;
            }

            // Prevent duplicate usernames (exclude current)
            if (!string.IsNullOrEmpty(staff.Username) && 
                _db.StaffSurveys_22D.Any(s => s.Username == staff.Username && s.Id != staff.Id))
            {
                ModelState.AddModelError("Username", "Username is already taken");
            }

            if (!ModelState.IsValid) return View(staff);

            existing.Name = staff.Name;
            existing.Username = staff.Username;
            existing.Password = staff.Password;
            existing.Email = staff.Email;
            existing.SatisfactionRate = staff.SatisfactionRate;
            existing.ProfessionalDevelopmentCount = staff.ProfessionalDevelopmentCount;

            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var staff = _db.StaffSurveys_22D.AsNoTracking().FirstOrDefault(s => s.Id == id);
            if (staff == null) return NotFound();
            return View(staff);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var staff = _db.StaffSurveys_22D.FirstOrDefault(s => s.Id == id);
            if (staff != null)
            {
                _db.StaffSurveys_22D.Remove(staff);
                _db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}