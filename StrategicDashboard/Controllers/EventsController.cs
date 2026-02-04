
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using OneJaxDashboard.Services;
//talijah

namespace OneJaxDashboard.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EventsService _events;
        private readonly StrategyService _strategyService;
        private readonly ActivityLogService _activityLog;

        public EventsController(ApplicationDbContext context, EventsService events, StrategyService strategyService, ActivityLogService activityLog)
        {
            _context = context;
            _events = events;
            _strategyService = strategyService;
            _activityLog = activityLog;
        }

        // GET: Events/Details/5 - Public access for dashboard
        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            try
            {
                // Try to get event from database first
                var eventFromDb = _context.Events?
                    .Where(e => e.Id == id)
                    .FirstOrDefault();

                if (eventFromDb != null)
                {
                    // Get the strategic goal name for ViewBag
                    var strategicGoal = _context.StrategicGoals?
                        .FirstOrDefault(g => g.Id == eventFromDb.StrategicGoalId);
                    ViewBag.GoalName = strategicGoal?.Name ?? "Strategic Goal";

                    return View(eventFromDb);
                }

                // Check if this is an event from the StrategyController
                var strategy = StrategyController.Strategies.FirstOrDefault(s => s.Id == id);
                if (strategy != null)
                {
                    // Convert Strategy to Event for display
                    var eventFromStrategy = new Event
                    {
                        Id = strategy.Id,
                        Title = strategy.Name,
                        Description = strategy.Description,
                        Type = strategy.EventType ?? "Community", // Use the event type from strategy
                        Status = "Planned",
                        StrategicGoalId = strategy.StrategicGoalId,
                        DueDate = DateTime.TryParse(strategy.Date, out var date) ? date : DateTime.Now.AddDays(30),
                        Notes = $"Added through Core Strategies tab. {(!string.IsNullOrEmpty(strategy.Time) ? $"Time: {strategy.Time}" : "")}",
                        Attendees = 0,
                        Location = string.IsNullOrEmpty(strategy.Time) ? "TBD" : $"Time: {strategy.Time}"
                    };

                    // Get goal name for ViewBag
                    var goalNames = new Dictionary<int, string>
                    {
                        {1, "Organizational Building"},
                        {2, "Financial Sustainability"},
                        {3, "Identity/Value Proposition"},
                        {4, "Community Engagement"}
                    };

                    ViewBag.GoalName = goalNames.ContainsKey(strategy.StrategicGoalId)
                        ? goalNames[strategy.StrategicGoalId]
                        : "Strategic Goal";

                    return View(eventFromStrategy);
                }
            }
            catch (Exception ex)
            {
                // Log error but continue
                Console.WriteLine($"[ERROR] EventsController.Details: {ex.Message}");
            }

            // Event not found
            return NotFound("Event not found. It may have been deleted or the ID is incorrect.");
        }

        [Authorize(Roles = "Staff")]
        public IActionResult Index()
        {
            var username = User.Identity?.Name ?? string.Empty;
            var items = _events.GetByOwner(username)
                .Where(e => e.StrategyTemplateId.HasValue && _strategyService.GetStrategy(e.StrategyTemplateId.Value) != null)
                .ToList();
            return View(items);
        }

        [Authorize(Roles = "Staff")]
        public IActionResult Archived()
        {
            var username = User.Identity?.Name ?? string.Empty;
            var items = _events.GetArchivedByOwner(username)
                .Where(e => e.StrategyTemplateId.HasValue && _strategyService.GetStrategy(e.StrategyTemplateId.Value) != null)
                .ToList();
            return View(items);
        }

        [Authorize(Roles = "Staff")]
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
        [Authorize(Roles = "Staff")]
        public IActionResult Create(Event eventModel)
        {
            // Load the strategy template to get the title
            var strategy = eventModel.StrategyTemplateId.HasValue ? _strategyService.GetStrategy(eventModel.StrategyTemplateId.Value) : null;
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
            var added = _events.Add(eventModel);
            _activityLog.Log(username, "Created Event", "Event", added.Id, notes: added.Title);
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Staff")]
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
        [Authorize(Roles = "Staff")]
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

        [Authorize(Roles = "Staff")]
        public IActionResult Delete(int id)
        {
            var eventModel = _events.Get(id);
            if (eventModel == null) return NotFound();
            if (!IsOwner(eventModel)) return Forbid();
            return View(eventModel);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff")]
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