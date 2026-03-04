using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
//Karrie's
namespace OneJaxDashboard.Controllers
{
    public class CommCollab47DController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommCollab47DController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CommCollab47D/Index
        [HttpGet]
        public IActionResult Index()
        {
            LoadStrategiesDropdown();

            var allEntries = _context.CollabTouch_47D
                .Include(c => c.Strategy)
                .OrderByDescending(c => c.CreatedDate)
                .ToList();

            ViewBag.TotalPartners = allEntries.Count;
            ViewBag.NewPartnersThisFY = allEntries
                .GroupBy(c => c.FiscalYear)
                .OrderByDescending(g => g.Key)
                .FirstOrDefault()?.Count() ?? 0;

            return View(new CollabTouch_47D());
        }

        // POST: CommCollab47D/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(CollabTouch_47D model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.CreatedDate = DateTime.Now;
                    _context.CollabTouch_47D.Add(model);
                    _context.SaveChanges();

                    TempData["Success"] = "Collaborative partner record submitted successfully!";
                    ViewBag.ShowNewEntryButton = true;
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error saving record: {ex.Message}";
                }
            }

            LoadStrategiesDropdown(model.StrategyId);
            return View(model);
        }

        // ── Helpers ──────────────────────────────────────────────────
        private void LoadStrategiesDropdown(int? selectedId = null)
        {
            ViewBag.Strategies = new SelectList(
                _context.Strategies.OrderBy(s => s.Name),
                "Id", "Name", selectedId);
        }
    }
}
