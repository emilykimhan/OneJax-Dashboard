using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
//Karrie's
namespace OneJaxDashboard.Controllers
{
    public class FinanceRate20DController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FinanceRate20DController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: FinanceRate20D/Index
        [HttpGet]
        public IActionResult Index()
        {
            // Calculate statistics
            var allEntries = _context.CommunicationRate.ToList();
            ViewBag.TotalEntries = allEntries.Count;
            
            if (allEntries.Any())
            {
                ViewBag.AverageSatisfaction = allEntries.Average(e => e.AverageCommunicationSatisfaction);
                ViewBag.LatestYear = allEntries.Max(e => e.Year);
            }
            
            return View(new Comm_rate20D());
        }

        // POST: FinanceRate20D/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(Comm_rate20D model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.CommunicationRate.Add(model);
                    _context.SaveChanges();
                    
                    // Recalculate statistics after adding new entry
                    var allEntries = _context.CommunicationRate.ToList();
                    ViewBag.TotalEntries = allEntries.Count;
                    ViewBag.AverageSatisfaction = allEntries.Average(e => e.AverageCommunicationSatisfaction);
                    ViewBag.LatestYear = allEntries.Max(e => e.Year);
                    
                    TempData["Success"] = "Communication satisfaction record submitted successfully!";
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
