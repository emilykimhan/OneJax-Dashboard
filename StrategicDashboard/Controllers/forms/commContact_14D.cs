using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
//Karrie's
namespace OneJaxDashboard.Controllers
{
    public class CommContact14DController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommContact14DController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CommContact14D/Index
        [HttpGet]
        public IActionResult Index()
        {
            LoadStats();
            return View(new ContactsInterfaith_14D());
        }

        // POST: CommContact14D/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(ContactsInterfaith_14D model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.CreatedDate = DateTime.Now;
                    _context.ContactsInterfaith_14D.Add(model);
                    _context.SaveChanges();

                    TempData["Success"] = "Interfaith contacts record submitted successfully!";
                    ViewBag.ShowNewEntryButton = true;
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error saving record: {ex.Message}";
                }
            }

            LoadStats();
            return View(model);
        }

        // ── Helpers ──────────────────────────────────────────────────
        private void LoadStats()
        {
            var allEntries = _context.ContactsInterfaith_14D
                .OrderBy(c => c.Year)
                .ToList();

            ViewBag.TotalEntries = allEntries.Count;

            // Calculate % growth between the two most recent years
            if (allEntries.Count >= 2)
            {
                var latest = allEntries.Last();
                var previous = allEntries[^2];

                if (previous.TotalInterfaithContacts > 0)
                {
                    double growth = ((double)(latest.TotalInterfaithContacts - previous.TotalInterfaithContacts)
                                    / previous.TotalInterfaithContacts) * 100;
                    ViewBag.GrowthPercent = Math.Round(growth, 1);
                }
                else
                {
                    ViewBag.GrowthPercent = null;
                }

                ViewBag.LatestYear = latest.Year;
                ViewBag.LatestTotal = latest.TotalInterfaithContacts;
            }
            else if (allEntries.Count == 1)
            {
                ViewBag.LatestYear = allEntries[0].Year;
                ViewBag.LatestTotal = allEntries[0].TotalInterfaithContacts;
                ViewBag.GrowthPercent = null;
            }
        }
    }
}
