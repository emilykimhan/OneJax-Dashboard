using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Models;

namespace OneJaxDashboard.Controllers
{
    public class DataEntryController : Controller
    {
        // Main page for Strategic Goals Data Entry
        [HttpGet]
        public IActionResult Index()
        {
            // Just load the main page (Index.cshtml under Views/DataEntry)
            return View();
        }

        // Redirect to OrganizationalBuilding controller 
        [HttpGet]
        public IActionResult OrganizationalBuilding()
        {
            return RedirectToAction("Index", "OrganizationalBuilding");
        }

        [HttpGet]
        public IActionResult StaffSurvey()
        {
            return RedirectToAction("Index", "StaffSurvey");
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
    }
}