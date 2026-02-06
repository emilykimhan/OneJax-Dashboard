using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
//Karrie's
namespace OneJaxDashboard.Controllers
{
    public class identityPlan_24DController : Controller
    {
        private readonly ApplicationDbContext _context;

        public identityPlan_24DController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: identityPlan_24D/Index
        [HttpGet]
        public IActionResult Index()
        {
            // Calculate statistics
            var allEntries = _context.Plan2026_24D.ToList();
            ViewBag.TotalEntries = allEntries.Count;
            
            if (allEntries.Any())
            {
                ViewBag.LatestYear = allEntries.Max(e => e.Year);
                ViewBag.GoalMetCount = allEntries.Count(e => e.GoalMet);
            }
            
            return View(new Plan2026_24D());
        }

        // POST: identityPlan_24D/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(Plan2026_24D model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.CreatedDate = DateTime.Now;
                    _context.Plan2026_24D.Add(model);
                    _context.SaveChanges();
                    
                    // Recalculate statistics after adding new entry
                    var allEntries = _context.Plan2026_24D.ToList();
                    ViewBag.TotalEntries = allEntries.Count;
                    ViewBag.LatestYear = allEntries.Max(e => e.Year);
                    ViewBag.GoalMetCount = allEntries.Count(e => e.GoalMet);
                    
                    TempData["Success"] = "Submitted successfully!";
                    ViewBag.ShowNewEntryButton = true;
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
