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
                ShowStaffSurveyButton = true,
            };

            return View(model);
        }

    
    }
}