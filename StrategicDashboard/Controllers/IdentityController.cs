using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Data;
using OneJaxDashboard.Services;
//karrie's
namespace OneJaxDashboard.Controllers
{
    public class IdentityController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;

        public IdentityController(ApplicationDbContext context, ActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }
        // GET: Identity/Index
        public IActionResult Index()
        {
            return View();
        }
    }
}
