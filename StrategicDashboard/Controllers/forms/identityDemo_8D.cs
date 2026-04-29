using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using OneJaxDashboard.Services;
//Karrie's
namespace OneJaxDashboard.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class identityDemo_8DController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;
        private readonly ILogger<identityDemo_8DController> _logger;
        private readonly SqlServerInsertCompatibilityService _sqlServerInsertCompatibility;

        public identityDemo_8DController(
            ApplicationDbContext context,
            ActivityLogService activityLog,
            ILogger<identityDemo_8DController> logger,
            SqlServerInsertCompatibilityService sqlServerInsertCompatibility)
        {
            _context = context;
            _activityLog = activityLog;
            _logger = logger;
            _sqlServerInsertCompatibility = sqlServerInsertCompatibility;
        }

        // GET: identityDemo_8D/Index
        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                LoadStrategiesDropdown();
                LoadStats();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load Program Demographics form data.");
                TempData["Error"] = BuildLoadErrorMessage("Program Demographics", ex);
                ViewBag.Strategies = EmptyStrategies();
            }

            return View(new demographics_8D { Year = DateTime.Now.Year });
        }

        // POST: identityDemo_8D/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(demographics_8D model)
        {
            if (ModelState.IsValid)
            {
                var zipCodes = model.ZipCodes
                    .Split(new[] { ',', ' ', '\r', '\n', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(z => z.Trim())
                    .Where(z => !string.IsNullOrWhiteSpace(z))
                    .ToList();

                if (!zipCodes.Any())
                {
                    ModelState.AddModelError("ZipCodes", "Please enter at least one ZIP code.");
                    LoadStrategiesDropdown(model.StrategyId);
                    TryLoadStats();
                    return View(model);
                }

                model.ZipCodes = string.Join(", ", zipCodes);

                try
                {
                    model.CreatedDate = DateTime.Now;
                    _sqlServerInsertCompatibility.PrepareForInsert(model, "demographics_8D");
                    _context.demographics_8D.Add(model);
                    _context.SaveChanges();
                    var actor = User?.Identity?.Name ?? "Unknown";
                    _activityLog.Log(actor, "Created Demographics Tracking Record", "Demographics",
                        details: $"Id={model.Id}");
                    
                    // Recalculate statistics after adding new entry
                    var allEntries = _context.demographics_8D.Include(d => d.Strategy).ToList();
                    ViewBag.TotalEntries = allEntries.Count;
                    ViewBag.LatestYear = allEntries.Max(e => e.Year);
                    
                    TempData["Success"] = "Submitted successfully!";
                    ViewBag.ShowNewEntryButton = true;
                    
                    LoadStrategiesDropdown(model.StrategyId);
                    
                    return View(model);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save Program Demographics record for strategy {StrategyId} year {Year}.",
                        model.StrategyId, model.Year);
                    TempData["Error"] = $"Error saving record: {ex.GetBaseException().Message}";
                }
            }

            LoadStrategiesDropdown(model.StrategyId);
            TryLoadStats();
            return View(model);
        }

        private void LoadStrategiesDropdown(int? selectedId = null)
        {
            try
            {
                ViewBag.Strategies = new SelectList(_context.Strategies.OrderBy(s => s.Name), "Id", "Name", selectedId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load strategies for Program Demographics.");
                TempData["Error"] ??= BuildLoadErrorMessage("Program Demographics", ex);
                ViewBag.Strategies = EmptyStrategies(selectedId);
            }
        }

        private void LoadStats()
        {
            var allEntries = _context.demographics_8D.Include(d => d.Strategy).ToList();
            ViewBag.TotalEntries = allEntries.Count;

            if (allEntries.Any())
            {
                ViewBag.LatestYear = allEntries.Max(e => e.Year);
            }
        }

        private void TryLoadStats()
        {
            try
            {
                LoadStats();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh Program Demographics stats.");
                TempData["Error"] ??= BuildLoadErrorMessage("Program Demographics", ex);
            }
        }

        private static SelectList EmptyStrategies(int? selectedId = null)
        {
            return new SelectList(Array.Empty<SelectListItem>(), "Value", "Text", selectedId);
        }

        private static string BuildLoadErrorMessage(string formName, Exception ex)
        {
            var message = ex.GetBaseException().Message;
            var schemaProblem =
                message.Contains("Invalid object name", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("Invalid column name", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("no such table", StringComparison.OrdinalIgnoreCase);

            return schemaProblem
                ? $"{formName} could not load because the Azure database schema is out of date."
                : $"{formName} could not load right now. Please try again.";
        }
    }
}
