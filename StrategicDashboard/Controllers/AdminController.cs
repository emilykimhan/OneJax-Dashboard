using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StrategicDashboard.Services;
using OneJaxDashboard.Data;

namespace StrategicDashboard.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AdminController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Admin
        public IActionResult Index()
        {
            var staffCount = _db.StaffMembers.Count();

            ViewData["StaffCount"] = staffCount;
            // Events management isn't persisted yet; link to DataEntry form
            return View();
        }
    }
}
