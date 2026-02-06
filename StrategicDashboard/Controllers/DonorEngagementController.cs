using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Data;
using OneJaxDashboard.Services;

namespace OneJaxDashboard.Controllers
{
    public class DonorEngagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;

        public DonorEngagementController(ApplicationDbContext context, ActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
