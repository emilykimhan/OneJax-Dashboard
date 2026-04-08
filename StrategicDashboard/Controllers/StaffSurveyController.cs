using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Models;
using OneJaxDashboard.Data;
using OneJaxDashboard.Services;
//karrie 
namespace OneJaxDashboard.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class StaffSurveyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;

        public StaffSurveyController(ApplicationDbContext context, ActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new StaffSurvey_22D());
        }

    
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(StaffSurvey_22D model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Add the survey data to the database
                    _context.StaffSurveys_22D.Add(model);
                    await _context.SaveChangesAsync();

                    var actor = User.Identity?.Name ?? "Unknown";
                    _activityLog.Log(actor, "Submitted Staff Satisfaction Survey", "StaffSurvey",
                        details: $"Id={model.Id}; Year: {model.Year}, Month: {model.Month}, Satisfaction: {model.SatisfactionRate}%");
                    
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
