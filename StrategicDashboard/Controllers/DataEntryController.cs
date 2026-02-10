using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Models;
using OneJaxDashboard.Data;
//Karrie
namespace OneJaxDashboard.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class DataEntryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DataEntryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RecordHistory(string recordType, string dateFilter, DateTime? startDate, DateTime? endDate)
        {
            // Load all records
            var allStaffSurveys = _context.StaffSurveys_22D.ToList();
            var allProfDev = _context.ProfessionalDevelopments.ToList();
            var allMediaPlacements = _context.MediaPlacements_3D.ToList();
            var allWebsiteTraffic = _context.WebsiteTraffic.ToList();
            
            // Apply filters
            var filteredStaffSurveys = allStaffSurveys;
            var filteredProfDev = allProfDev;
            var filteredMediaPlacements = allMediaPlacements;
            var filteredWebsiteTraffic = allWebsiteTraffic;
            
            // Filter by date
            DateTime filterStartDate = DateTime.MinValue;
            DateTime filterEndDate = DateTime.MaxValue;
            
            if (!string.IsNullOrEmpty(dateFilter))
            {
                switch (dateFilter)
                {
                    case "today":
                        filterStartDate = DateTime.Today;
                        filterEndDate = DateTime.Today.AddDays(1).AddSeconds(-1);
                        break;
                    case "week":
                        filterStartDate = DateTime.Today.AddDays(-7);
                        filterEndDate = DateTime.Now;
                        break;
                    case "month":
                        filterStartDate = DateTime.Today.AddDays(-30);
                        filterEndDate = DateTime.Now;
                        break;
                    case "custom":
                        if (startDate.HasValue) filterStartDate = startDate.Value;
                        if (endDate.HasValue) filterEndDate = endDate.Value.AddDays(1).AddSeconds(-1);
                        break;
                }
                
                if (dateFilter != "all")
                {
                    filteredStaffSurveys = filteredStaffSurveys
                        .Where(s => s.CreatedDate >= filterStartDate && s.CreatedDate <= filterEndDate)
                        .ToList();
                    filteredProfDev = filteredProfDev
                        .Where(p => p.CreatedDate >= filterStartDate && p.CreatedDate <= filterEndDate)
                        .ToList();
                    filteredMediaPlacements = filteredMediaPlacements
                        .Where(m => m.CreatedDate >= filterStartDate && m.CreatedDate <= filterEndDate)
                        .ToList();
                    filteredWebsiteTraffic = filteredWebsiteTraffic
                        .Where(w => w.CreatedDate >= filterStartDate && w.CreatedDate <= filterEndDate)
                        .ToList();
                }
            }
            
            // Filter by record type
            if (recordType == "staff-survey")
            {
                filteredProfDev = new List<ProfessionalDevelopment>();
                filteredMediaPlacements = new List<MediaPlacements_3D>();
                filteredWebsiteTraffic = new List<WebsiteTraffic_4D>();
            }
            else if (recordType == "professional-development")
            {
                filteredStaffSurveys = new List<StaffSurvey_22D>();
                filteredMediaPlacements = new List<MediaPlacements_3D>();
                filteredWebsiteTraffic = new List<WebsiteTraffic_4D>();
            }
            else if (recordType == "media-placements")
            {
                filteredStaffSurveys = new List<StaffSurvey_22D>();
                filteredProfDev = new List<ProfessionalDevelopment>();
                filteredWebsiteTraffic = new List<WebsiteTraffic_4D>();
            }
            else if (recordType == "website-traffic")
            {
                filteredStaffSurveys = new List<StaffSurvey_22D>();
                filteredProfDev = new List<ProfessionalDevelopment>();
                filteredMediaPlacements = new List<MediaPlacements_3D>();
            }
            
            // Set ViewBag data
            ViewBag.StaffSurveys = filteredStaffSurveys;
            ViewBag.ProfessionalDevelopments = filteredProfDev;
            ViewBag.MediaPlacements = filteredMediaPlacements;
            ViewBag.WebsiteTraffic = filteredWebsiteTraffic;
            ViewBag.RecordType = recordType ?? "all";
            ViewBag.DateFilter = dateFilter ?? "all";
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.TotalCount = allStaffSurveys.Count + allProfDev.Count + allMediaPlacements.Count + allWebsiteTraffic.Count;
            ViewBag.VisibleCount = filteredStaffSurveys.Count + filteredProfDev.Count + filteredMediaPlacements.Count + filteredWebsiteTraffic.Count;
            
            return View();
        }

        // Delete Staff Survey
        [HttpPost]
        public IActionResult DeleteStaffSurvey(int id)
        {
            var survey = _context.StaffSurveys_22D.Find(id);
            if (survey != null)
            {
                _context.StaffSurveys_22D.Remove(survey);
                _context.SaveChanges();
                TempData["Success"] = "Staff Survey record deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Record not found.";
            }
            return RedirectToAction("RecordHistory");
        }

        // Delete Professional Development
        [HttpPost]
        public IActionResult DeleteProfessionalDevelopment(int id)
        {
            var profDev = _context.ProfessionalDevelopments.Find(id);
            if (profDev != null)
            {
                _context.ProfessionalDevelopments.Remove(profDev);
                _context.SaveChanges();
                TempData["Success"] = "Professional Development record deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Record not found.";
            }
            return RedirectToAction("RecordHistory");
        }

        // Delete Media Placement
        [HttpPost]
        public IActionResult DeleteMediaPlacement(int id)
        {
            var media = _context.MediaPlacements_3D.Find(id);
            if (media != null)
            {
                _context.MediaPlacements_3D.Remove(media);
                _context.SaveChanges();
                TempData["Success"] = "Media Placement record deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Record not found.";
            }
            return RedirectToAction("RecordHistory");
        }

        // Delete Website Traffic
        [HttpPost]
        public IActionResult DeleteWebsiteTraffic(int id)
        {
            var traffic = _context.WebsiteTraffic.Find(id);
            if (traffic != null)
            {
                _context.WebsiteTraffic.Remove(traffic);
                _context.SaveChanges();
                TempData["Success"] = "Website Traffic record deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Record not found.";
            }
            return RedirectToAction("RecordHistory");
        }

        // Edit Staff Survey - GET
        [HttpGet]
        public IActionResult EditStaffSurvey(int id)
        {
            var survey = _context.StaffSurveys_22D.Find(id);
            if (survey == null)
            {
                TempData["Error"] = "Record not found.";
                return RedirectToAction("RecordHistory");
            }
            return View(survey);
        }

        // Edit Staff Survey - POST
        [HttpPost]
        public IActionResult EditStaffSurvey(StaffSurvey_22D model)
        {
            if (ModelState.IsValid)
            {
                var survey = _context.StaffSurveys_22D.Find(model.Id);
                if (survey != null)
                {
                    survey.Name = model.Name;
                    survey.SatisfactionRate = model.SatisfactionRate;
                    survey.ProfessionalDevelopmentCount = model.ProfessionalDevelopmentCount;
                    _context.SaveChanges();
                    TempData["Success"] = "Staff Survey record updated successfully!";
                    return RedirectToAction("RecordHistory");
                }
                else
                {
                    TempData["Error"] = "Record not found.";
                }
            }
            return View(model);
        }

        // Edit Professional Development - GET
        [HttpGet]
        public IActionResult EditProfessionalDevelopment(int id)
        {
            var profDev = _context.ProfessionalDevelopments.Find(id);
            if (profDev == null)
            {
                TempData["Error"] = "Record not found.";
                return RedirectToAction("RecordHistory");
            }
            return View(profDev);
        }

        // Edit Professional Development - POST
        [HttpPost]
        public IActionResult EditProfessionalDevelopment(ProfessionalDevelopment model)
        {
            if (ModelState.IsValid)
            {
                var profDev = _context.ProfessionalDevelopments.Find(model.Id);
                if (profDev != null)
                {
                    profDev.Name = model.Name;
                    profDev.ProfessionalDevelopmentYear26 = model.ProfessionalDevelopmentYear26;
                    profDev.ProfessionalDevelopmentYear27 = model.ProfessionalDevelopmentYear27;
                    _context.SaveChanges();
                    TempData["Success"] = "Professional Development record updated successfully!";
                    return RedirectToAction("RecordHistory");
                }
                else
                {
                    TempData["Error"] = "Record not found.";
                }
            }
            return View(model);
        }

        // Edit Media Placement - GET
        [HttpGet]
        public IActionResult EditMediaPlacement(int id)
        {
            var media = _context.MediaPlacements_3D.Find(id);
            if (media == null)
            {
                TempData["Error"] = "Record not found.";
                return RedirectToAction("RecordHistory");
            }
            return View(media);
        }

        // Edit Media Placement - POST
        [HttpPost]
        public IActionResult EditMediaPlacement(MediaPlacements_3D model)
        {
            if (ModelState.IsValid)
            {
                var media = _context.MediaPlacements_3D.Find(model.Id);
                if (media != null)
                {
                    media.January = model.January;
                    media.February = model.February;
                    media.March = model.March;
                    media.April = model.April;
                    media.May = model.May;
                    media.June = model.June;
                    media.July = model.July;
                    media.August = model.August;
                    media.September = model.September;
                    media.October = model.October;
                    media.November = model.November;
                    media.December = model.December;
                    _context.SaveChanges();
                    TempData["Success"] = "Media Placement record updated successfully!";
                    return RedirectToAction("RecordHistory");
                }
                else
                {
                    TempData["Error"] = "Record not found.";
                }
            }
            return View(model);
        }

        // Edit Website Traffic - GET
        [HttpGet]
        public IActionResult EditWebsiteTraffic(int id)
        {
            var traffic = _context.WebsiteTraffic.Find(id);
            if (traffic == null)
            {
                TempData["Error"] = "Record not found.";
                return RedirectToAction("RecordHistory");
            }
            return View(traffic);
        }

        // Edit Website Traffic - POST
        [HttpPost]
        public IActionResult EditWebsiteTraffic(WebsiteTraffic_4D model)
        {
            if (ModelState.IsValid)
            {
                var traffic = _context.WebsiteTraffic.Find(model.Id);
                if (traffic != null)
                {
                    traffic.Q1_JulySeptember = model.Q1_JulySeptember;
                    traffic.Q2_OctoberDecember = model.Q2_OctoberDecember;
                    traffic.Q3_JanuaryMarch = model.Q3_JanuaryMarch;
                    traffic.Q4_AprilJune = model.Q4_AprilJune;
                    _context.SaveChanges();
                    TempData["Success"] = "Website Traffic record updated successfully!";
                    return RedirectToAction("RecordHistory");
                }
                else
                {
                    TempData["Error"] = "Record not found.";
                }
            }
            return View(model);
        }
    }
}