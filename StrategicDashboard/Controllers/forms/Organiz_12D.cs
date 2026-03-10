using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Models;
using OneJaxDashboard.Data;
using OneJaxDashboard.Services;

namespace OneJaxDashboard.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class EventSatisfaction12DController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;

        public EventSatisfaction12DController(ApplicationDbContext context, ActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }

        // GET: EventSatisfaction12D/Index
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Strategies = GetStrategies();
            return View(new eventSatisfaction());
        }

        // POST: EventSatisfaction12D/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(eventSatisfaction model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.EventSatisfaction_12D.Add(model);
                    _context.SaveChanges();
                    var actor = User?.Identity?.Name ?? "Unknown";
                    _activityLog.Log(actor, "Created Event Satisfaction Record", "EventSatisfaction",
                        details: $"Id={model.Id}");
                    
                    TempData["Success"] = "Event satisfaction record submitted successfully!";
                    ViewBag.ShowNewEntryButton = true;
                    ViewBag.Strategies = GetStrategies();
                    return View(model);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error saving record: {ex.Message}";
                }
            }

            ViewBag.Strategies = GetStrategies();
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
    }
}
