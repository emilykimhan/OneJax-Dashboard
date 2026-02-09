using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Models;
using OneJaxDashboard.Data;

namespace OneJaxDashboard.Controllers
{
    public class Income27DController : Controller
    {
        private readonly ApplicationDbContext _context;

        public Income27DController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Income27D/Index
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Months = GetMonths();
            
            // Calculate totals
            var allEntries = _context.income_27D.ToList();
            var grandTotal = allEntries.Sum(e => e.Amount);
            
            ViewBag.GrandTotal = grandTotal;
            ViewBag.TotalEntries = allEntries.Count;
            
            return View(new income_27D());
        }

        // POST: Income27D/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(income_27D model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.income_27D.Add(model);
                    _context.SaveChanges();
                    
                    // Recalculate totals after adding new entry
                    var allEntries = _context.income_27D.ToList();
                    var grandTotal = allEntries.Sum(e => e.Amount);
                    
                    ViewBag.GrandTotal = grandTotal;
                    ViewBag.TotalEntries = allEntries.Count;
                    
                    TempData["Success"] = "Income record submitted successfully!";
                    ViewBag.ShowNewEntryButton = true;
                    ViewBag.Months = GetMonths();
                    return View(model);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error saving record: {ex.Message}";
                }
            }

            ViewBag.Months = GetMonths();
            return View(model);
        }

        // GET: Income27D/List
        [HttpGet]
        public IActionResult List()
        {
            var entries = _context.income_27D       
                .OrderByDescending(e => e.CreatedDate)
                .ToList();
            
            ViewBag.GrandTotal = entries.Sum(e => e.Amount);
            
            return View(entries);
        }

        // GET: Income27D/MonthlyReport
        [HttpGet]
        public IActionResult MonthlyReport()
        {
            var monthlyTotals = _context.income_27D
                .GroupBy(i => i.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Total = g.Sum(i => i.Amount),
                    Count = g.Count()
                })
                .OrderBy(m => m.Month)
                .ToList();
            
            ViewBag.GrandTotal = monthlyTotals.Sum(m => m.Total);
            
            return View(monthlyTotals);
        }

        private List<SelectListItem> GetMonths()
        {
            var currentYear = DateTime.Now.Year;
            var months = new List<SelectListItem>();
            
            for (int year = currentYear - 1; year <= currentYear + 2; year++)
            {
                var monthNames = new[] { "January", "February", "March", "April", "May", "June", 
                                        "July", "August", "September", "October", "November", "December" };
                
                foreach (var month in monthNames)
                {
                    months.Add(new SelectListItem 
                    { 
                        Value = $"{month} {year}", 
                        Text = $"{month} {year}" 
                    });
                }
            }
            
            return months;
        }
    }
}
