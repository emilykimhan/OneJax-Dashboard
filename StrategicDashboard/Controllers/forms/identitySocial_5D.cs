using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;

namespace OneJaxDashboard.Controllers
{
    public class identitySocial_5DController : Controller
    {
        private readonly ApplicationDbContext _context;

        public identitySocial_5DController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: identitySocial_5D/Index
        [HttpGet]
        public IActionResult Index()
        {
            // Calculate statistics
            var allEntries = _context.socialMedia_5D.ToList();
            ViewBag.TotalEntries = allEntries.Count;
            
            if (allEntries.Any())
            {
                ViewBag.AverageEngagement = allEntries.Average(e => e.AverageEngagementRate);
                ViewBag.GoalMetCount = allEntries.Count(e => e.GoalMet);
                ViewBag.LatestYear = allEntries.Max(e => e.Year);
            }
            
            return View(new socialMedia_5D());
        }

        // POST: identitySocial_5D/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(socialMedia_5D model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.CreatedDate = DateTime.Now;
                    _context.socialMedia_5D.Add(model);
                    _context.SaveChanges();
                    
                    TempData["Success"] = "Social media engagement data submitted successfully!";
                    return RedirectToAction("Index");
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
