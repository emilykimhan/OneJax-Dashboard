using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
//Karrie's
namespace OneJaxDashboard.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class CommFaith13DController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommFaith13DController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CommFaith13D/Index
        [HttpGet]
        public IActionResult Index()
        {
            LoadStrategiesDropdown();

            var allEntries = _context.FaithCommunity_13D
                .Include(f => f.Strategy)
                .ToList();

            ViewBag.TotalEvents = allEntries.Count;
            ViewBag.EventsMeetingGoal = allEntries.Count(f => f.NumberOfFaithsRepresented >= 3);
            ViewBag.PercentMeetingGoal = allEntries.Count > 0
                ? Math.Round((double)allEntries.Count(f => f.NumberOfFaithsRepresented >= 3) / allEntries.Count * 100, 1)
                : 0;

            return View(new FaithCommunity_13D());
        }

        // POST: CommFaith13D/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(FaithCommunity_13D model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.CreatedDate = DateTime.Now;
                    _context.FaithCommunity_13D.Add(model);
                    _context.SaveChanges();

                    TempData["Success"] = "Faith community record submitted successfully!";
                    ViewBag.ShowNewEntryButton = true;
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error saving record: {ex.Message}";
                }
            }

            LoadStrategiesDropdown(model.StrategyId);

            var allEntries = _context.FaithCommunity_13D.ToList();
            ViewBag.TotalEvents = allEntries.Count;
            ViewBag.EventsMeetingGoal = allEntries.Count(f => f.NumberOfFaithsRepresented >= 3);
            ViewBag.PercentMeetingGoal = allEntries.Count > 0
                ? Math.Round((double)allEntries.Count(f => f.NumberOfFaithsRepresented >= 3) / allEntries.Count * 100, 1)
                : 0;

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
