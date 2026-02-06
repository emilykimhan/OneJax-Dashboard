using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
//Karrie's
namespace OneJaxDashboard.Controllers
{
    public class identityDemo_8DController : Controller
    {
        private readonly ApplicationDbContext _context;

        public identityDemo_8DController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: identityDemo_8D/Index
        [HttpGet]
        public IActionResult Index()
        {
            // Load strategies for dropdown
            ViewBag.Strategies = new SelectList(_context.Strategies.OrderBy(s => s.Name), "Id", "Name");

            // Calculate statistics
            var allEntries = _context.demographics_8D.Include(d => d.Strategy).ToList();
            ViewBag.TotalEntries = allEntries.Count;
            
            if (allEntries.Any())
            {
                ViewBag.LatestYear = allEntries.Max(e => e.Year);
            }
            
            return View(new demographics_8D());
        }

        // POST: identityDemo_8D/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(demographics_8D model)
        {
            if (ModelState.IsValid)
            {
                // Validate no duplicate zip codes
                var zipCodes = model.ZipCodes.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                             .Select(z => z.Trim())
                                             .ToList();
                
                var duplicates = zipCodes.GroupBy(z => z)
                                        .Where(g => g.Count() > 1)
                                        .Select(g => g.Key)
                                        .ToList();
                
                if (duplicates.Any())
                {
                    ModelState.AddModelError("ZipCodes", $"Duplicate zip codes found: {string.Join(", ", duplicates)}");
                    ViewBag.Strategies = new SelectList(_context.Strategies.OrderBy(s => s.Name), "Id", "Name");
                    return View(model);
                }

                try
                {
                    model.CreatedDate = DateTime.Now;
                    _context.demographics_8D.Add(model);
                    _context.SaveChanges();
                    
                    // Recalculate statistics after adding new entry
                    var allEntries = _context.demographics_8D.Include(d => d.Strategy).ToList();
                    ViewBag.TotalEntries = allEntries.Count;
                    ViewBag.LatestYear = allEntries.Max(e => e.Year);
                    
                    TempData["Success"] = "Submitted successfully!";
                    ViewBag.ShowNewEntryButton = true;
                    
                    // Reload strategies for dropdown
                    ViewBag.Strategies = new SelectList(_context.Strategies.OrderBy(s => s.Name), "Id", "Name");
                    
                    return View(model);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error saving record: {ex.Message}";
                }
            }

            // Reload strategies for dropdown in case of validation error
            ViewBag.Strategies = new SelectList(_context.Strategies.OrderBy(s => s.Name), "Id", "Name");
            return View(model);
        }
    }
}
