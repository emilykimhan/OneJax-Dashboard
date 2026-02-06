using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Data;
using OneJaxDashboard.Services;

namespace OneJaxDashboard.Controllers
{
    public class CommunityEngagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;

        public CommunityEngagementController(ApplicationDbContext context, ActivityLogService activityLog)
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
