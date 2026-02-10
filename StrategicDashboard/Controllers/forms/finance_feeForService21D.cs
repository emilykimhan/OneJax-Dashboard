using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Models;
using OneJaxDashboard.Data;
//Karrie's
namespace OneJaxDashboard.Controllers
{
    public class FeeForService21DController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FeeForService21DController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: FeeForService21D/Index
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Strategies = GetStrategies();
            ViewBag.Quarters = GetQuarters();
            return View(new feeForService_21D());
        }

        // POST: FeeForService21D/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(feeForService_21D model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Store the program title name for easier reporting
                    var strategy = _context.Strategies.Find(model.StrategyId);
                    if (strategy != null)
                    {
                        model.ProgramTitle = strategy.Name;
                    }
                    
                    _context.FeeForServices_21D.Add(model);
                    _context.SaveChanges();
                    
                    TempData["Success"] = "Submitted successfully!";
                    ViewBag.ShowNewEntryButton = true;
                    ViewBag.Strategies = GetStrategies();
                    ViewBag.Quarters = GetQuarters();
                    return View(model);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error saving record: {ex.Message}";
                }
            }

            ViewBag.Strategies = GetStrategies();
            ViewBag.Quarters = GetQuarters();
            return View(model);
        }

        private List<SelectListItem> GetStrategies()
        {
            return _context.Strategies
                .Select(s => new SelectListItem 
                { 
                    Value = s.Id.ToString(), 
                    Text = s.Name 
                })
                .ToList();
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
