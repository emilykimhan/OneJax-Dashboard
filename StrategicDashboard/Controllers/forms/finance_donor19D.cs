using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Models;
using OneJaxDashboard.Data;
//Karrie's
namespace OneJaxDashboard.Controllers
{
    public class DonorEvent19DController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DonorEvent19DController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DonorEvent19D/Index
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Strategies = GetStrategies();
            return View(new DonorEvent_19D());
        }

        // POST: DonorEvent19D/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(DonorEvent_19D model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.DonorEvents_19D.Add(model);
                    _context.SaveChanges();
                    
                    TempData["Success"] = "Donor event record submitted successfully!";
                    ViewBag.ShowNewEntryButton = true;
                    ViewBag.Strategies = GetStrategies();
                    return View(model);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error saving record: {ex.Message}";
                }
            }

            ViewBag.Strategies = GetStrategies();
            return View(model);
        }

        private List<SelectListItem> GetStrategies()
        {
            return _context.Strategies
                .Select(s => new SelectListItem 
                { 
                    Value = s.Id.ToString(), 
                    Text = s.Name 
                })
                .ToList();
        }
    }
}
