using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Models;
using OneJaxDashboard.Data;

namespace OneJaxDashboard.Controllers
{
    public class EventSatisfaction12DController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventSatisfaction12DController(ApplicationDbContext context)
        {
            _context = context;
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
