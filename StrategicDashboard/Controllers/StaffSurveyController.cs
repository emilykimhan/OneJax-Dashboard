using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OneJaxDashboard.Models;
using OneJaxDashboard.Data;

namespace OneJaxDashboard.Controllers
{
    public class StaffSurveyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StaffSurveyController(ApplicationDbContext context)
        {
            _context = context;
        }
        private List<SelectListItem> GetStaffMembers()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "Elizabeth Andersen", Text = "Elizabeth Andersen" },
                new SelectListItem { Value = "Cilicia Anderson", Text = "Cilicia Anderson" },
                new SelectListItem { Value = "Jacey Kelly", Text = "Jacey Kelly" },
                new SelectListItem { Value = "Deidre Lane", Text = "Deidre Lane" },
                new SelectListItem { Value = "Jan Phillips", Text = "Jan Phillips" },
                new SelectListItem { Value = "Rebekah Hutton", Text = "Rebekah Hutton" }
            };
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
                    
                    TempData["SuccessMessage"] = "Survey submitted and saved successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    // Log the error (if logging is set up)
                    TempData["ErrorMessage"] = "An error occurred while saving the survey. Please try again.";
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