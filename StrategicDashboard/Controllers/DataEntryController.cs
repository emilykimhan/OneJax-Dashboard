using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Models;
using OneJaxDashboard.Data;

namespace OneJaxDashboard.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class DataEntryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DataEntryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Main page for Strategic Goals Data Entry
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // Redirect to OrganizationalBuilding controller 
        [HttpGet]
        public IActionResult OrganizationalBuilding()
        {
            return RedirectToAction("Index", "OrganizationalBuilding");
        }


        [HttpGet]
        public IActionResult Identity()
        {
            ViewData["Title"] = "Identity / Value Proposition";
            return View();
        }

        [HttpGet]
        public IActionResult Community()
        {
            ViewData["Title"] = "Community Engagement";
            return View();
        }
        [HttpGet]
        public IActionResult Financial()
        {
            ViewData["Title"] = "Financial Sustainability";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> RecordHistory()
        {
            ViewData["Title"] = "Record History";
            
            // Get all records for Record History page
            var staffSurveys = await _context.StaffSurveys_22D.ToListAsync();
            var professionalDevelopments = await _context.ProfessionalDevelopments.ToListAsync();
            
            ViewBag.StaffSurveys = staffSurveys;
            ViewBag.ProfessionalDevelopments = professionalDevelopments;
            
            return View();
        }
    }
}