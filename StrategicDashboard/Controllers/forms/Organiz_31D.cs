using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using OneJaxDashboard.Services;

namespace OneJaxDashboard.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class SelfAssessmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _activityLog;

        public SelfAssessmentController(ApplicationDbContext context, ActivityLogService activityLog)
        {
            _context = context;
            _activityLog = activityLog;
        }

        // GET: SelfAssessment/Index
        [HttpGet]
        public IActionResult Index()
        {
            var allEntries = _context.selfAssess_31D
                .OrderByDescending(s => s.Year)
                .ToList();
            
            var averageScore = allEntries.Any() ? allEntries.Average(s => s.SelfAssessmentScore) : 0;
            var currentYearEntry = allEntries.FirstOrDefault(s => s.Year == DateTime.Now.Year);
            
            ViewBag.AverageScore = Math.Round(averageScore, 1);
            ViewBag.CurrentYearScore = currentYearEntry?.SelfAssessmentScore ?? 0;
            ViewBag.TotalEntries = allEntries.Count;
            ViewBag.AllEntries = allEntries;
            
            return View(new selfAssess_31D());
        }

        // POST: SelfAssessment/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(selfAssess_31D model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.selfAssess_31D.Add(model);
                    _context.SaveChanges();
                    var actor = User?.Identity?.Name ?? "Unknown";
                    _activityLog.Log(actor, "Created Board Self-Assessment Record", "SelfAssessment",
                        details: $"Id={model.Id}; Year: {model.Year}; Month: {model.Month}; Score: {model.SelfAssessmentScore}");
                    
                    TempData["Success"] = "Board self-assessment record submitted successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error saving record: {ex.Message}";
                }
            }

            var allEntries = _context.selfAssess_31D
                .OrderByDescending(s => s.Year)
                .ToList();
            
            ViewBag.AverageScore = allEntries.Any() ? Math.Round(allEntries.Average(s => s.SelfAssessmentScore), 1) : 0;
            ViewBag.CurrentYearScore = allEntries.FirstOrDefault(s => s.Year == DateTime.Now.Year)?.SelfAssessmentScore ?? 0;
            ViewBag.TotalEntries = allEntries.Count;
            ViewBag.AllEntries = allEntries;
            
            return View(model);
        }
    }
}
