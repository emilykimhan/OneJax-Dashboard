using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OneJaxDashboard.Models;
using OneJaxDashboard.Services;
//talijah
namespace OneJaxDashboard.Controllers
{
    [Authorize(Roles = "Staff")]
    public class EventsController : Controller
    {
        private readonly EventsService _events;
        private readonly StrategyService _strategyService;
        private readonly ActivityLogService _activityLog;

        public EventsController(EventsService events, StrategyService strategyService, ActivityLogService activityLog)
        {
            _events = events;
            _strategyService = strategyService;
            _activityLog = activityLog;
        }

        public IActionResult Index()
        {
            var username = User.Identity?.Name ?? string.Empty;
            var items = _events.GetByOwner(username)
                .Where(e => _strategyService.GetStrategy(e.StrategyTemplateId) != null)
                .ToList();
            return View(items);
        }

        public IActionResult Archived()
        {
            var username = User.Identity?.Name ?? string.Empty;
            var items = _events.GetArchivedByOwner(username)
                .Where(e => _strategyService.GetStrategy(e.StrategyTemplateId) != null)
                .ToList();
            return View(items);
        }

        public IActionResult Create(int? strategyId)
        {
            Event eventModel = new Event();
            
            // If a strategy template is selected, load its data
            if (strategyId.HasValue)
            {
                var strategy = _strategyService.GetStrategy(strategyId.Value);
                if (strategy != null)
                {
                    eventModel.StrategyTemplateId = strategy.Id;
                    eventModel.Title = strategy.Name;
                    eventModel.Description = strategy.Description;
                    eventModel.StrategicGoalId = strategy.StrategicGoalId;
                    eventModel.StrategyId = strategy.Id;
                }
            }
            
            PopulateStrategicGoalsAndStrategies(strategyId);
            return View(eventModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Event eventModel)
        {
            // Load the strategy template to get the title
            var strategy = _strategyService.GetStrategy(eventModel.StrategyTemplateId);
            if (strategy == null)
            {
                ModelState.AddModelError("", "Please select an event.");
                PopulateStrategicGoalsAndStrategies(null);
                return View(eventModel);
            }
            
            if (!ModelState.IsValid) 
            {
                PopulateStrategicGoalsAndStrategies(null);
                return View(eventModel);
            }
            
            // Set title and related fields from the strategy template
            eventModel.Title = strategy.Name;
            eventModel.StrategicGoalId = strategy.StrategicGoalId;
            eventModel.StrategyId = strategy.Id;
            
            var username = User.Identity?.Name ?? string.Empty;
            eventModel.OwnerUsername = username;
            _events.Add(eventModel);
            _activityLog.Log(username, "Created Event", "Event", eventModel.Id, notes: eventModel.Title);
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var eventModel = _events.Get(id);
            if (eventModel == null) return NotFound();
            if (!IsOwner(eventModel)) return Forbid();
            
            PopulateStrategicGoalsAndStrategies(null);
            return View(eventModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Event eventModel)
        {
            var existing = _events.Get(eventModel.Id);
            if (existing == null) return NotFound();
            if (!IsOwner(existing)) return Forbid();
            
            if (!ModelState.IsValid) 
            {
                PopulateStrategicGoalsAndStrategies(null);
                return View(eventModel);
            }
            
            // Keep owner unchanged
            eventModel.OwnerUsername = existing.OwnerUsername;
            
            // Log status changes specifically
            var statusChanged = existing.Status != eventModel.Status;
            var statusChangeNote = statusChanged ? $"Status changed from '{existing.Status}' to '{eventModel.Status}'" : null;
            
            // Auto-archive when status is Completed
            if (eventModel.Status == "Completed")
            {
                eventModel.IsArchived = true;
                eventModel.CompletionDate = DateTime.Now;
            }
            
            _events.Update(eventModel);
            
            // Log the update with status change detail if applicable
            var logAction = statusChanged ? "Changed Event Status" : "Updated Event";
            var logNotes = statusChangeNote ?? eventModel.Title;
            _activityLog.Log(existing.OwnerUsername, logAction, "Event", eventModel.Id, notes: logNotes);
            
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var eventModel = _events.Get(id);
            if (eventModel == null) return NotFound();
            if (!IsOwner(eventModel)) return Forbid();
            return View(eventModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var eventModel = _events.Get(id);
            if (eventModel == null) return NotFound();
            if (!IsOwner(eventModel)) return Forbid();
            _events.Remove(id);
            _activityLog.Log(eventModel.OwnerUsername, "Deleted Event", "Event", id, notes: eventModel.Title);
            return RedirectToAction("Index");
        }

        private bool IsOwner(Event e)
        {
            var username = User.Identity?.Name ?? string.Empty;
            return string.Equals(e.OwnerUsername, username, StringComparison.OrdinalIgnoreCase);
        }

        private void PopulateStrategicGoalsAndStrategies(int? selectedStrategyId)
        {
            var goals = _strategyService.GetAllStrategicGoals();
            ViewBag.StrategicGoals = new SelectList(goals, "Id", "Name");
            
            // Get all strategies from Core Strategies to use as event templates
            var allStrategies = _strategyService.GetAllStrategies();
            ViewBag.StrategyTemplates = new SelectList(allStrategies, "Id", "Name", selectedStrategyId);
            ViewBag.SelectedStrategyId = selectedStrategyId;
            
            // Load all strategies for display
            ViewBag.Strategies = new SelectList(allStrategies, "Id", "Name");
        }
    }
}