using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OneJaxDashboard.Models;
using OneJaxDashboard.Data;
//karrie's
namespace OneJaxDashboard.Controllers
{
    public class ProfessionalDevelopmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProfessionalDevelopmentController(ApplicationDbContext context)
        {
            _context = context;
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


//Replace GetStaffMembers with the staff database, when all the staff accounts have been made or started
        private List<SelectListItem> GetStaffMembers()
        {
            // Clean slate - staff data will come from database
            return new List<SelectListItem>();
        }
    }
}