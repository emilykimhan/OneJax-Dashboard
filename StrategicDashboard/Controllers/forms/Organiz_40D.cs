using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using Microsoft.EntityFrameworkCore;

namespace OneJaxDashboard.Controllers
{
    public class VolunteerProgramController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VolunteerProgramController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: VolunteerProgram/Index
        [HttpGet]
        public IActionResult Index()
        {
            var allEntries = _context.volunteerProgram_40D
                .OrderByDescending(v => v.Year)
                .ThenByDescending(v => v.Quarter)
                .ToList();
            
            var totalVolunteers = allEntries.Any() ? allEntries.OrderByDescending(v => v.CreatedDate).FirstOrDefault()?.NumberOfVolunteers ?? 0 : 0;
            var totalInitiatives = allEntries.Sum(v => v.VolunteerLedInitiatives);
            
            ViewBag.TotalVolunteers = totalVolunteers;
            ViewBag.TotalInitiatives = totalInitiatives;
            ViewBag.TotalEntries = allEntries.Count;
            
            return View(new volunteerProgram_40D());
        }

        // POST: VolunteerProgram/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(volunteerProgram_40D model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.volunteerProgram_40D.Add(model);
                    _context.SaveChanges();
                    
                    TempData["Success"] = "Volunteer program record submitted successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error saving record: {ex.Message}";
                }
            }

            var allEntries = _context.volunteerProgram_40D
                .OrderByDescending(v => v.Year)
                .ThenByDescending(v => v.Quarter)
                .ToList();
            
            ViewBag.TotalVolunteers = allEntries.Any() ? allEntries.OrderByDescending(v => v.CreatedDate).FirstOrDefault()?.NumberOfVolunteers ?? 0 : 0;
            ViewBag.TotalInitiatives = allEntries.Sum(v => v.VolunteerLedInitiatives);
            ViewBag.TotalEntries = allEntries.Count;
            
            return View(model);
        }
    }
}
