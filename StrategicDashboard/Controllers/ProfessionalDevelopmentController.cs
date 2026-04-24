using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Models;
using OneJaxDashboard.Data;
using OneJaxDashboard.Services;
//karrie's
namespace OneJaxDashboard.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class ProfessionalDevelopmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;
        private readonly ILogger<ProfessionalDevelopmentController> _logger;

        public ProfessionalDevelopmentController(
            ApplicationDbContext context,
            ActivityLogService activityLog,
            ILogger<ProfessionalDevelopmentController> logger)
        {
            _context = context;
            _activityLog = activityLog;
            _logger = logger;
        }

        
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.StaffMembers = GetStaffMembers();
            return View(new ProfessionalDevelopment());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(ProfessionalDevelopment model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.ProfessionalDevelopments.Add(model);
                    _context.SaveChanges();

                    var actor = User.Identity?.Name ?? "Unknown";
                    _activityLog.Log(actor, "Submitted Professional Development Survey", "ProfessionalDevelopment",
                        details: $"Id={model.Id}; Staff member: {model.Name}; Year: {model.Year}; Month: {model.Month}");
                    TempData["Success"] = "Professional development record submitted successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    var rootCause = ex.GetBaseException().Message;
                    _logger.LogError(ex, "Failed to save professional development record for {Name} ({Year}/{Month}).",
                        model.Name, model.Year, model.Month);
                    TempData["Error"] = $"Error saving record: {rootCause}";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while saving professional development record for {Name} ({Year}/{Month}).",
                        model.Name, model.Year, model.Month);
                    TempData["Error"] = $"Error saving record: {ex.Message}";
                }
            }

            ViewBag.StaffMembers = GetStaffMembers();
            return View(model);
        }


        private List<SelectListItem> GetStaffMembers()
        {
            return _context.Staffauth
                .Select(s => new SelectListItem 
                { 
                    Value = s.Name, 
                    Text = s.Name 
                })
                .ToList();
        }
    }
}
