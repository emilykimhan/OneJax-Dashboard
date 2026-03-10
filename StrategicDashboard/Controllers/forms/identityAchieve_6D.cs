using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;

namespace OneJaxDashboard.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class identityAchieve_6DController : Controller
    {
        private readonly ApplicationDbContext _context;

        public identityAchieve_6DController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: identityAchieve_6D/Index
        [HttpGet]
        public IActionResult Index()
        {
            // Calculate statistics
            var allEntries = _context.achieveMile_6D.ToList();
            ViewBag.TotalEntries = allEntries.Count;
            
            if (allEntries.Any())
            {
                ViewBag.AveragePercentage = allEntries.Average(e => e.Percentage);
                ViewBag.GoalMetCount = allEntries.Count(e => e.GoalMet);
                ViewBag.SixMonthReviewCount = allEntries.Count(e => e.achievedReview);
            }
            
            return View(new achieveMile_6D());
        }

        // POST: identityAchieve_6D/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(achieveMile_6D model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.CreatedDate = DateTime.Now;
                    _context.achieveMile_6D.Add(model);
                    _context.SaveChanges();
                    
                    // Recalculate statistics after adding new entry
                    var allEntries = _context.achieveMile_6D.ToList();
                    ViewBag.TotalEntries = allEntries.Count;
                    ViewBag.AveragePercentage = allEntries.Average(e => e.Percentage);
                    ViewBag.GoalMetCount = allEntries.Count(e => e.GoalMet);
                    ViewBag.SixMonthReviewCount = allEntries.Count(e => e.achievedReview);
                    
                    TempData["Success"] = "Milestone achievement record submitted successfully!";
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
