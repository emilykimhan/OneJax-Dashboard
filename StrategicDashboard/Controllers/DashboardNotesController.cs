using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Services;

namespace OneJaxDashboard.Controllers;

[Route("DashboardNotes")]
public sealed class DashboardNotesController : Controller
{
    private readonly IDashboardNotesStore _store;

    public DashboardNotesController(IDashboardNotesStore store)
    {
        _store = store;
    }

    [HttpGet("Get")]
    [AllowAnonymous]
    public IActionResult Get([FromQuery] string key)
    {
        var value = _store.Get(key) ?? string.Empty;
        return Json(new { key, value });
    }

    [HttpPost("Save")]
    [Authorize(Roles = "Admin,Staff")]
    [ValidateAntiForgeryToken]
    public IActionResult Save([FromForm] string key, [FromForm] string value, [FromForm] string? returnUrl)
    {
        _store.Set(key, value);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost("SaveJson")]
    [Authorize(Roles = "Admin,Staff")]
    [ValidateAntiForgeryToken]
    public IActionResult SaveJson([FromForm] string key, [FromForm] string value)
    {
        _store.Set(key, value);
        return Json(new { ok = true, key, value = (value ?? string.Empty).Trim() });
    }
}
