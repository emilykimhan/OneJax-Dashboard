using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
//karrie's
namespace OneJaxDashboard.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class IdentityController : Controller
    {
        // GET: /Identity (Form hub)
        public IActionResult Index()
        {
            return View();
        }

        // GET: /ValueProposition (safe alias for Identity hub)
        [HttpGet("/ValueProposition")]
        public IActionResult ValueProposition()
        {
            return RedirectToAction(nameof(Index));
        }

        // GET: /Identity/Dashboard (Dashboard shortcut)
        [HttpGet]
        public IActionResult Dashboard()
        {
            return RedirectToAction("Index", "Home", new { goal = "Identity/Value Proposition" });
        }
    }
}
