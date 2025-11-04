using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OneJaxDashboard.Models;
using StrategicDashboard.Data;

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