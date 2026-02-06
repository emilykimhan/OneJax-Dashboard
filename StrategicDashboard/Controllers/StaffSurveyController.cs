using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Models;
using OneJaxDashboard.Data;
using OneJaxDashboard.Services;
//karrie 
namespace OneJaxDashboard.Controllers
{
    public class StaffSurveyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;

        public StaffSurveyController(ApplicationDbContext context, ActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }
        private List<SelectListItem> GetStaffMembers()
        {
            return _context.Staffauth
                .Select(s => new SelectListItem 
                { 
                    Value = s.Name, 
                    Text = s.Name 
                })
                .ToList();
        }

   
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.StaffMembers = GetStaffMembers();
            return View(new StaffSurvey_22D());
        }

    
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(StaffSurvey_22D model)
        {
            ViewBag.StaffMembers = GetStaffMembers();

            if (ModelState.IsValid)
            {
                try
                {
                    // Add the survey data to the database
                    _context.StaffSurveys_22D.Add(model);
                    await _context.SaveChangesAsync();
                    
                    // Log the activity
                    var username = User.Identity?.Name ?? "Unknown";
                    _activityLog.Log(username, "Created Staff Survey", "StaffSurvey_22D", model.Id, $"Survey for {model.Name}");
                    
                    TempData["SuccessMessage"] = "Survey submitted and saved successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception exception) 
                {
                    // Log the error (if logging is set up)
                    TempData["ErrorMessage"] = exception.Message;
                    return View(model);
                }
            }

            // If validation fails, redisplay form
            return View(model);
        }

        [HttpGet]
        public IActionResult Survey()
        {
            return RedirectToAction("Index");
        }
    }
}