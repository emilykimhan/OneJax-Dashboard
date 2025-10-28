using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Models;

namespace OneJaxDashboard.Controllers
{
    public class OrganizationalBuildingController : Controller
    {
       
        [HttpGet]
        public IActionResult Index()
        {
            var model = new OrganizationalBuildingViewModel
            {
                PageTitle = "Organizational Building",
                StaffSurveyDescription = "Track staff satisfaction and professional development activities.",
                PlayDescription = "Engage in team building activities, games, and recreational resources for staff development.",
                ShowStaffSurveyButton = true,
                ShowPlaySection = true
            };

            return View(model);
        }

        [HttpGet]
        public IActionResult Play()
        {
       
            ViewData["Title"] = "Play Activities";
            return View();
        }

        [HttpGet]
        public IActionResult Reports()
        {
            
            ViewData["Title"] = "Organizational Reports";
            return View();
        }
    }
}