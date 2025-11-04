using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OneJaxDashboard.Models;
using OneJaxDashboard.Data;

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
        public async Task<IActionResult> Index(ProfessionalDevelopment model)
        {
            ViewBag.StaffMembers = GetStaffMembers();
            
            if (ModelState.IsValid)
            {
                try
                {
                    // Save to database
                    _context.ProfessionalDevelopments.Add(model);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = $"Professional development record for {model.Name} submitted successfully! Year 2026: {model.ProfessionalDevelopmentYear26} activities, Year 2027: {model.ProfessionalDevelopmentYear27} activities.";
                    return RedirectToAction("Index");
                }
                catch (Exception)
                {
                    TempData["Error"] = "An error occurred while saving the professional development record. Please try again.";
                    return View(model);
                }
            }

           
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