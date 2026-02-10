using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;

namespace OneJaxDashboard.Controllers
{
    public class FaithRepresentationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FaithRepresentationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: FaithRepresentation/Index
        [HttpGet]
        public IActionResult Index()
        {
            // Load all strategies for dropdown
            ViewBag.Strategies = new SelectList(_context.Strategies, "Id", "Name");
            
            // Calculate statistics
            var allEntries = _context.FaithRepresentations
                .Include(f => f.Strategy)
                .ToList();
            
            var totalEvents = allEntries.Count;
            var eventsWithThreePlus = allEntries.Count(e => e.NumberOfFaiths >= 3);
            var percentageWith3Faiths = totalEvents > 0 
                ? (eventsWithThreePlus * 100.0 / totalEvents) 
                : 0;
            
            ViewBag.TotalEvents = totalEvents;
            ViewBag.EventsWith3PlusFaiths = eventsWithThreePlus;
            ViewBag.PercentageWith3Faiths = percentageWith3Faiths;
            ViewBag.GoalPercentage = 80; // Goal is 80%
            ViewBag.MeetsGoal = percentageWith3Faiths >= 80;
            
            return View(new FaithRepres_13D());
        }

        // POST: FaithRepresentation/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(FaithRepres_13D model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Get the strategy/event name from the selected strategy
                    var strategy = _context.Strategies.Find(model.StrategyId);
                    if (strategy != null)
                    {
                        model.EventName = strategy.Name;
                    }
                    
                    model.CreatedDate = DateTime.Now;
                    _context.FaithRepresentations.Add(model);
                    _context.SaveChanges();
                    
                    // Recalculate statistics
                    var allEntries = _context.FaithRepresentations
                        .Include(f => f.Strategy)
                        .ToList();
                    
                    var totalEvents = allEntries.Count;
                    var eventsWithThreePlus = allEntries.Count(e => e.NumberOfFaiths >= 3);
                    var percentageWith3Faiths = totalEvents > 0 
                        ? (eventsWithThreePlus * 100.0 / totalEvents) 
                        : 0;
                    
                    ViewBag.TotalEvents = totalEvents;
                    ViewBag.EventsWith3PlusFaiths = eventsWithThreePlus;
                    ViewBag.PercentageWith3Faiths = percentageWith3Faiths;
                    ViewBag.GoalPercentage = 80;
                    ViewBag.MeetsGoal = percentageWith3Faiths >= 80;
                    
                    TempData["Success"] = "Faith representation record submitted successfully!";
                    ViewBag.ShowNewEntryButton = true;
                    
                    // Reload strategies for dropdown
                    ViewBag.Strategies = new SelectList(_context.Strategies, "Id", "Name");
                    
                    return View(model);
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error saving record: {ex.Message}";
                }
            }
            
            // Reload strategies for dropdown
            ViewBag.Strategies = new SelectList(_context.Strategies, "Id", "Name");
            return View(model);
        }

        // GET: FaithRepresentation/ViewAll
        [HttpGet]
        public IActionResult ViewAll()
        {
            var entries = _context.FaithRepresentations
                .Include(f => f.Strategy)
                .OrderByDescending(f => f.CreatedDate)
                .ToList();
            
            // Calculate statistics
            var totalEvents = entries.Count;
            var eventsWithThreePlus = entries.Count(e => e.NumberOfFaiths >= 3);
            var percentageWith3Faiths = totalEvents > 0 
                ? (eventsWithThreePlus * 100.0 / totalEvents) 
                : 0;
            
            ViewBag.TotalEvents = totalEvents;
            ViewBag.EventsWith3PlusFaiths = eventsWithThreePlus;
            ViewBag.PercentageWith3Faiths = percentageWith3Faiths;
            ViewBag.GoalPercentage = 80;
            ViewBag.MeetsGoal = percentageWith3Faiths >= 80;
            
            return View(entries);
        }

        // GET: FaithRepresentation/Edit/5
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var entry = _context.FaithRepresentations.Find(id);
            if (entry == null)
            {
                return NotFound();
            }
            
            ViewBag.Strategies = new SelectList(_context.Strategies, "Id", "Name", entry.StrategyId);
            return View(entry);
        }

        // POST: FaithRepresentation/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, FaithRepres_13D model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the strategy/event name from the selected strategy
                    var strategy = _context.Strategies.Find(model.StrategyId);
                    if (strategy != null)
                    {
                        model.EventName = strategy.Name;
                    }
                    
                    _context.Update(model);
                    _context.SaveChanges();
                    
                    TempData["Success"] = "Record updated successfully!";
                    return RedirectToAction(nameof(ViewAll));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FaithRepresentationExists(model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error updating record: {ex.Message}";
                }
            }
            
            ViewBag.Strategies = new SelectList(_context.Strategies, "Id", "Name", model.StrategyId);
            return View(model);
        }

        // GET: FaithRepresentation/Delete/5
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var entry = _context.FaithRepresentations
                .Include(f => f.Strategy)
                .FirstOrDefault(f => f.Id == id);
            
            if (entry == null)
            {
                return NotFound();
            }
            
            return View(entry);
        }

        // POST: FaithRepresentation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                var entry = _context.FaithRepresentations.Find(id);
                if (entry != null)
                {
                    _context.FaithRepresentations.Remove(entry);
                    _context.SaveChanges();
                    TempData["Success"] = "Record deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Record not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting record: {ex.Message}";
            }
            
            return RedirectToAction(nameof(ViewAll));
        }

        private bool FaithRepresentationExists(int id)
        {
            return _context.FaithRepresentations.Any(e => e.Id == id);
        }
    }
}
