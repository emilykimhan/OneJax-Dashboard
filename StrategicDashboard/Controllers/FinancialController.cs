using Microsoft.AspNetCore.Mvc;

namespace OneJaxDashboard.Controllers
{
    public class FinancialController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
