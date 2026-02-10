using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Models;
using OneJaxDashboard.Data;

namespace OneJaxDashboard.Controllers
{
    public class BudgetTracking28DController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BudgetTracking28DController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: BudgetTracking28D/Index
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Quarters = GetQuarters();
            
            // Calculate actual budget totals from database
            var budgetEntries = _context.BudgetTracking_28D.ToList();
            var totalRevenue = budgetEntries.Sum(b => b.TotalRevenues);
            var totalExpense = budgetEntries.Sum(b => b.TotalExpenses);
            
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalExpense = totalExpense;
            
            return View(new BudgetTracking_28D());
        }

        // POST: BudgetTracking28D/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(BudgetTracking_28D model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.BudgetTracking_28D.Add(model);
                    _context.SaveChanges();
                    
                    TempData["Success"] = "Budget tracking record submitted successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error saving record: {ex.Message}";
                }
            }

            ViewBag.Quarters = GetQuarters();
            
            // Calculate totals for display even on validation errors
            var budgetEntries = _context.BudgetTracking_28D.ToList();
            ViewBag.TotalRevenue = budgetEntries.Sum(b => b.TotalRevenues);
            ViewBag.TotalExpense = budgetEntries.Sum(b => b.TotalExpenses);
            
            return View(model);
        }

        // GET: BudgetTracking28D/List
        [HttpGet]
        public IActionResult List()
        {
            var entries = _context.BudgetTracking_28D
                .OrderByDescending(e => e.Year)
                .ThenByDescending(e => e.Quarter)
                .ToList();
            
            var totalRevenue = entries.Sum(e => e.TotalRevenues);
            var totalExpense = entries.Sum(e => e.TotalExpenses);
            
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalExpense = totalExpense;
            ViewBag.NetTotal = totalRevenue - totalExpense;
            
            return View(entries);
        }

        // GET: BudgetTracking28D/QuarterlyReport
        [HttpGet]
        public IActionResult QuarterlyReport()
        {
            var quarterlyData = _context.BudgetTracking_28D
                .OrderByDescending(e => e.Year)
                .ThenByDescending(e => e.Quarter)
                .ToList();
            
            var totalRevenue = quarterlyData.Sum(e => e.TotalRevenues);
            var totalExpense = quarterlyData.Sum(e => e.TotalExpenses);
            
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalExpense = totalExpense;
            ViewBag.NetTotal = totalRevenue - totalExpense;
            ViewBag.AnnualBudgetRevenue = 994600m;
            ViewBag.AnnualBudgetExpense = 960200m;
            
            return View(quarterlyData);
        }

        private List<SelectListItem> GetQuarters()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "Q1", Text = "Q1 (January - March)" },
                new SelectListItem { Value = "Q2", Text = "Q2 (April - June)" },
                new SelectListItem { Value = "Q3", Text = "Q3 (July - September)" },
                new SelectListItem { Value = "Q4", Text = "Q4 (October - December)" }
            };
        }
    }
}
