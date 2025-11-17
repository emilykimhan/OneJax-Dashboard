using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StrategicDashboard.Models;
using StrategicDashboard.Services;

namespace StrategicDashboard.Controllers
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
            var items = _events.GetByOwner(username);
            return View(items);
        }

        public IActionResult Create()
        {
            PopulateStrategicGoalsAndStrategies();
            return View(new Event());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Event eventModel)
        {
            if (!ModelState.IsValid) 
            {
                PopulateStrategicGoalsAndStrategies();
                return View(eventModel);
            }
            
            var username = User.Identity?.Name ?? string.Empty;
            eventModel.OwnerUsername = username;
            var added = _events.Add(eventModel);
            _activityLog.Log(username, "Created Event", "Event", added.Id, notes: added.Title);
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var eventModel = _events.Get(id);
            if (eventModel == null) return NotFound();
            if (!IsOwner(eventModel)) return Forbid();
            
            PopulateStrategicGoalsAndStrategies();
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
                PopulateStrategicGoalsAndStrategies();
                return View(eventModel);
            }
            
            // Keep owner unchanged
            eventModel.OwnerUsername = existing.OwnerUsername;
            _events.Update(eventModel);
            _activityLog.Log(existing.OwnerUsername, "Updated Event", "Event", eventModel.Id, notes: eventModel.Title);
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

        [HttpGet]
        public IActionResult GetStrategiesByGoal(int goalId)
        {
            var strategies = _strategyService.GetStrategiesByGoal(goalId);
            return Json(strategies.Select(s => new { value = s.Id, text = s.Name }));
        }

        private bool IsOwner(Event e)
        {
            var username = User.Identity?.Name ?? string.Empty;
            return string.Equals(e.OwnerUsername, username, StringComparison.OrdinalIgnoreCase);
        }

        private void PopulateStrategicGoalsAndStrategies()
        {
            var goals = _strategyService.GetAllStrategicGoals();
            ViewBag.StrategicGoals = new SelectList(goals, "Id", "Name");
            
            // Empty strategies list - will be populated via JavaScript based on selected goal
            ViewBag.Strategies = new SelectList(Enumerable.Empty<Strategy>(), "Id", "Name");
        }
    }
}