using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Models;
using OneJaxDashboard.Data;
using OneJaxDashboard.Services;

//karrie's
namespace OneJaxDashboard.Controllers
{
    public class OrganizationalBuildingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;

        public OrganizationalBuildingController(ApplicationDbContext context, ActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }

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