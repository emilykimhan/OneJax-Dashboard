using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using OneJaxDashboard.Services;

namespace OneJaxDashboard.Controllers
{
    public class OrganizationalBuildingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;

        public OrganizationalBuildingController(ApplicationDbContext context, ActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new OrganizationalBuilding());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(OrganizationalBuilding model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.OrganizationalBuildings.Add(model);
                    _context.SaveChanges();
                    
                    // Log the activity
                    var username = User.Identity?.Name ?? "Unknown";
                    _activityLog.Log(username, "Created Organizational Building Record", "OrganizationalBuilding", model.Id, 
                        notes: $"Added organizational building record");
                    
                    TempData["Success"] = "Organizational building record submitted successfully!";
                    return RedirectToAction(nameof(Index));
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