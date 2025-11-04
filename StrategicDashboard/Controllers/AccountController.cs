using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using StrategicDashboard.Models;
using OneJaxDashboard.Data;
using System.Security.Claims;

namespace StrategicDashboard.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AccountController(ApplicationDbContext db)
        {
            _db = db;
        }
        // GET: /Account/Login
        [HttpGet]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public IActionResult Login()
        {
            // If user is already authenticated, redirect to appropriate page
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Admin");
                }
                // Staff already-authenticated browsing to login -> go to Staff dashboard
                return RedirectToAction("Index", "StaffPortal");
            }
            
            return View(new LoginViewModel());
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);
            var usernameLower = model.Username.ToLower();

            // Admin hard-coded login
            if (usernameLower == "admin" && model.Password == "Admin123!")
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.Username),
                    new Claim(ClaimTypes.Role, "Admin")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe
                    });

                // If a local returnUrl is provided (e.g., redirected by [Authorize]), go there
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Admin");
            }

            // Staff login against database
            var staff = _db.StaffMembers.FirstOrDefault(s => s.Username == model.Username);
            if (staff != null && staff.Password == model.Password)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, staff.Username),
                    new Claim(ClaimTypes.Role, "Staff"),
                    new Claim(ClaimTypes.GivenName, staff.FullName ?? string.Empty)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe
                    });

                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "StaffPortal");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}