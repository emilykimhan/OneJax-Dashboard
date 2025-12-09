using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
//karrie's
namespace OneJaxDashboard.Controllers
{
    public class MediaPlacementsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MediaPlacementsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MediaPlacements/Index
        [HttpGet]
        public IActionResult Index()
        {
            // Calculate grand total across all entries
            var allEntries = _context.MediaPlacements_3D.ToList();
            var grandTotal = allEntries.Sum(e => e.TotalMentions);
            
            ViewBag.GrandTotal = grandTotal;
            ViewBag.TotalEntries = allEntries.Count;
            
            return View(new MediaPlacements_3D());
        }

        // POST: MediaPlacements/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(MediaPlacements_3D model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.MediaPlacements_3D.Add(model);
                    _context.SaveChanges();
                    
                    // Recalculate grand total after adding new entry
                    var allEntries = _context.MediaPlacements_3D.ToList();
                    var grandTotal = allEntries.Sum(e => e.TotalMentions);
                    
                    ViewBag.GrandTotal = grandTotal;
                    ViewBag.TotalEntries = allEntries.Count;
                    
                    TempData["Success"] = "Media placements record submitted successfully!";
                    ViewBag.ShowNewEntryButton = true;
                    return View(model); // Show the submitted data instead of redirecting
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
