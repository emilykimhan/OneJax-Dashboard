using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Models;
using OneJaxDashboard.Data;
//karrie 
namespace OneJaxDashboard.Controllers
{
    public class StaffSurveyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StaffSurveyController(ApplicationDbContext context)
        {
            _context = context;
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