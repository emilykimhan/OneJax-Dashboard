using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Models;
using OneJaxDashboard.Data;
using OneJaxDashboard.Services;
//Karrie's
namespace OneJaxDashboard.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class FeeForService21DController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;

        public FeeForService21DController(ApplicationDbContext context, ActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
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
                        model.EventName = strategy.Name;
                    }
                    
                    _context.FeeForServices_21D.Add(model);
                    _context.SaveChanges();

                    var actor = User.Identity?.Name ?? "Unknown";
                    _activityLog.Log(actor, "Created Fee-for-Service Revenue Record", "FeeForService", model.Id);
                    
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
