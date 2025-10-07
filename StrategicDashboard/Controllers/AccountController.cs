using Microsoft.AspNetCore.Mvc;
using StrategicDashboard.Models;

namespace StrategicDashboard.Controllers
{
    public class AccountController : Controller
    {
        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            
            if (!string.IsNullOrEmpty(model.Username) && !string.IsNullOrEmpty(model.Password))
            {
                
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }
    }
}