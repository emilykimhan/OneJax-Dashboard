using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Data;
using OneJaxDashboard.Services;
using OneJaxDashboard.Models;

namespace OneJaxDashboard.Controllers
{
    public class YouthAttendanceController : Controller
    {
         private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;

        public YouthAttendanceController(ApplicationDbContext context, ActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
