using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
//Karrie's
namespace OneJaxDashboard.Controllers
{
    public class WebsiteTrafficController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WebsiteTrafficController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: WebsiteTraffic/Index
        [HttpGet]
        public IActionResult Index()
        {
            // Calculate grand total across all entries
            var allEntries = _context.WebsiteTraffic.ToList();
            var grandTotal = allEntries.Sum(e => e.TotalClicks);
            
            ViewBag.GrandTotal = grandTotal;
            ViewBag.TotalEntries = allEntries.Count;
            
            return View(new WebsiteTraffic_4D());
        }

        // POST: WebsiteTraffic/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(WebsiteTraffic_4D model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.WebsiteTraffic.Add(model);
                    _context.SaveChanges();
                    
                    // Recalculate grand total after adding new entry
                    var allEntries = _context.WebsiteTraffic.ToList();
                    var grandTotal = allEntries.Sum(e => e.TotalClicks);
                    
                    ViewBag.GrandTotal = grandTotal;
                    ViewBag.TotalEntries = allEntries.Count;
                    
                    TempData["Success"] = "Website traffic record submitted successfully!";
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
