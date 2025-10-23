using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Models;

namespace OneJax_Dashboard.Controllers
{
    public class DataEntryController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View(new EventEntryViewModel());
        }

        [HttpPost]
        public IActionResult Index(EventEntryViewModel model)
        {
            if (ModelState.IsValid)
            {
                // TODO: Save to database
                TempData["SuccessMessage"] = "Event successfully submitted!";
                return RedirectToAction("Index");
            }

            // If validation fails, redisplay form with errors
            return View(model);
        }
    }
}