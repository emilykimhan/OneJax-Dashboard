using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OneJaxDashboard.Models;
using OneJaxDashboard.Data;
using OneJaxDashboard.Services;
//karrie's
namespace OneJaxDashboard.Controllers
{
    public class ProfessionalDevelopmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;

        public ProfessionalDevelopmentController(ApplicationDbContext context, ActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }

        
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.StaffMembers = GetStaffMembers();
            return View(new ProfessionalDevelopment());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(ProfessionalDevelopment model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.ProfessionalDevelopments.Add(model);
                    _context.SaveChanges();
                    
                    // Log the activity
                    var username = User.Identity?.Name ?? "Unknown";
                    _activityLog.Log(username, "Created Professional Development", "ProfessionalDevelopment", model.Id, 
                        notes: $"Added professional development for {model.StaffMember}");
                    
                    TempData["Success"] = "Professional development record submitted successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error saving record: {ex.Message}";
                }
            }

            ViewBag.StaffMembers = GetStaffMembers();
            return View(model);
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
    }
}