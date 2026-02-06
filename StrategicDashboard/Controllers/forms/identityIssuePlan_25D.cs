using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
//Karrie's
namespace OneJaxDashboard.Controllers
{
    public class identityIssuePlan_25DController : Controller
    {
        private readonly ApplicationDbContext _context;

        public identityIssuePlan_25DController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: identityIssuePlan_25D/Index
        [HttpGet]
        public IActionResult Index()
        {
            // Load plans for dropdown
            ViewBag.Plans = new SelectList(_context.Plan2026_24D.OrderBy(p => p.Name), "Id", "Name");

            // Calculate statistics
            var allEntries = _context.planIssue_25D.Include(p => p.Plan).ToList();
            ViewBag.TotalIncidents = allEntries.Count;
            
            if (allEntries.Any())
            {
                ViewBag.CompliantIncidents = allEntries.Count(e => e.IsCompliant);
                ViewBag.CompliancePercentage = Math.Round((decimal)ViewBag.CompliantIncidents / ViewBag.TotalIncidents * 100, 2);
                ViewBag.GoalMet = ViewBag.CompliancePercentage >= 90;
                ViewBag.LatestYear = allEntries.Max(e => e.Year);
            }
            
            return View(new planIssue_25D());
        }

        // POST: identityIssuePlan_25D/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(planIssue_25D model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.CreatedDate = DateTime.Now;
                    _context.planIssue_25D.Add(model);
                    _context.SaveChanges();
                    
                    // Recalculate statistics after adding new entry
                    var allEntries = _context.planIssue_25D.Include(p => p.Plan).ToList();
                    ViewBag.TotalIncidents = allEntries.Count;
                    ViewBag.CompliantIncidents = allEntries.Count(e => e.IsCompliant);
                    ViewBag.CompliancePercentage = Math.Round((decimal)ViewBag.CompliantIncidents / ViewBag.TotalIncidents * 100, 2);
                    ViewBag.GoalMet = ViewBag.CompliancePercentage >= 90;
                    ViewBag.LatestYear = allEntries.Max(e => e.Year);
                    
                    TempData["Success"] = "Submitted successfully!";
                    ViewBag.ShowNewEntryButton = true;
                    
                    // Reload plans for dropdown
                    ViewBag.Plans = new SelectList(_context.Plan2026_24D.OrderBy(p => p.Name), "Id", "Name");
                    
                    return View(model);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error saving record: {ex.Message}";
                }
            }

            // Reload plans for dropdown in case of validation error
            ViewBag.Plans = new SelectList(_context.Plan2026_24D.OrderBy(p => p.Name), "Id", "Name");
            return View(model);
        }
    }
}
