using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Models;
using OneJaxDashboard.Data;
using OneJaxDashboard.Services;

namespace OneJaxDashboard.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class Interfaith11DController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;

        public Interfaith11DController(ApplicationDbContext context, ActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }

        // GET: Interfaith11D/Index
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Strategies = GetStrategies();
            return View(new interfaith_11D());
        }

        // POST: Interfaith11D/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(interfaith_11D model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Interfaith_11D.Add(model);
                    _context.SaveChanges();
                    var actor = User?.Identity?.Name ?? "Unknown";
                    _activityLog.Log(actor, "Created Interfaith Event Record", "InterfaithEvent",
                        details: $"Id={model.Id}");
                    
                    TempData["Success"] = "Interfaith event record submitted successfully!";
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
