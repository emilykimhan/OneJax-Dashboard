using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
//Karrie's
namespace OneJaxDashboard.Controllers
{
    public class CrossSector10DController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CrossSector10DController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CrossSector10D/Index
        [HttpGet]
        public IActionResult Index()
        {
            // Calculate statistics
            var allEntries = _context.CrossSectorCollabs.ToList();
            ViewBag.TotalEntries = allEntries.Count();
            
            if (allEntries.Any())
            {
                ViewBag.ActiveCollaborations = allEntries.Count(e => e.Status == "Active");
                ViewBag.InactiveCollaborations = allEntries.Count(e => e.Status == "Inactive");
                ViewBag.LatestYear = allEntries.Max(e => e.Year);
            }
            
            return View(new CrossSector10D());
        }

        // POST: CrossSector10D/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(CrossSector10D model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if a record already exists for this collaboration name and year
                    var existingEntry = _context.CrossSectorCollabs
                        .FirstOrDefault(e => e.Name == model.Name && e.Year == model.Year);
                    
                    if (existingEntry != null)
                    {
                        TempData["Error"] = $"A record for '{model.Name}' in year {model.Year} already exists. Each collaboration can only have one entry per year.";
                        return View(model);
                    }

                    _context.CrossSectorCollabs.Add(model);
                    _context.SaveChanges();
                    
                    // Recalculate statistics after adding new entry
                    var allEntries = _context.CrossSectorCollabs.ToList();
                    ViewBag.TotalEntries = allEntries.Count();
                    ViewBag.ActiveCollaborations = allEntries.Count(e => e.Status == "Active");
                    ViewBag.InactiveCollaborations = allEntries.Count(e => e.Status == "Inactive");
                    ViewBag.LatestYear = allEntries.Max(e => e.Year);
                    
                    TempData["Success"] = "Cross-sector collaboration record submitted successfully!";
                    return View(model);
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
