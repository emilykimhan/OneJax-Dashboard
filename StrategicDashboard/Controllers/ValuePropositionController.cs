using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OneJaxDashboard.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    [Route("ValueProposition")]
    public class ValuePropositionController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("~/Views/Identity/Index.cshtml");
        }

        [HttpGet("Dashboard")]
        public IActionResult Dashboard()
        {
            return RedirectToAction("Index", "Home", new { goal = "Identity/Value Proposition" });
        }
    }
}
