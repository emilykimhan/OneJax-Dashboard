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
        private readonly TimeZoneInfo _easternTimeZone;
        
        private DateTime NowEastern => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _easternTimeZone);

        public EventsController(ApplicationDbContext context, EventsService events, StrategyService strategyService, ActivityLogService activityLog, TimeZoneInfo easternTimeZone)
        {
            _context = context;
            _events = events;
            _strategyService = strategyService;
            _activityLog = activityLog;
            _easternTimeZone = easternTimeZone;
        }

        // GET: Events/Details/5 - Public access for dashboard
        [AllowAnonymous]
        public IActionResult Details(int id)
        {
            try
            {
                // Try to get event from database first
                var eventFromDb = _context.Events?
                    .Include(e => e.AssignedStaff)
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
            }
            catch (Exception ex)
            {
                // Log error but continue
                Console.WriteLine($"[ERROR] EventsController.Details: {ex.Message}");
            }

            // Event not found
            return NotFound("Event not found. It may have been deleted or the ID is incorrect.");
        }

        // GET: Events/Api/Details/5 - Ultra-fast API endpoint for modal
        [AllowAnonymous]
        [Route("Events/Api/Details/{id}")]
        [Route("api/events/details/{id}")]
        [HttpGet]
        public IActionResult ApiDetails(int id)
        {
            try
            {
                var goalNames = new Dictionary<int, string>
                {
                    { 1, "Organizational Building" },
                    { 2, "Financial Sustainability" },
                    { 3, "Identity/Value Proposition" },
                    { 4, "Community Engagement" }
                };

                var eventFromDb = _context.Events
                    .Where(e => e.Id == id)
                    .Select(e => new
                    {
                        e.Id,
                        e.Title,
                        e.Description,
                        e.Type,
                        e.Status,
                        e.DueDate,
                        e.Location,
                        e.Attendees,
                        e.Notes,
                        e.SatisfactionScore,
                        e.StrategicGoalId
                    })
                    .FirstOrDefault();

                if (eventFromDb == null)
                {
                    return Json(new { error = "Event not found" });
                }

                var goalName = eventFromDb.StrategicGoalId.HasValue
                               && goalNames.TryGetValue(eventFromDb.StrategicGoalId.Value, out var resolvedName)
                    ? resolvedName
                    : "Strategic Goal";

                return Json(new
                {
                    Id = eventFromDb.Id,
                    Title = eventFromDb.Title,
                    Description = eventFromDb.Description,
                    Type = eventFromDb.Type,
                    Status = eventFromDb.Status,
                    DueDate = eventFromDb.DueDate?.ToString("MMMM dd, yyyy"),
                    Location = eventFromDb.Location,
                    Attendees = eventFromDb.Attendees,
                    Notes = eventFromDb.Notes,
                    SatisfactionScore = eventFromDb.SatisfactionScore,
                    GoalName = goalName,
                    Source = "Database"
                });
            }
            catch (Exception)
            {
                return Json(new { error = "Failed to load event details" });
            }
        }

        // Original API method (backup)
        [AllowAnonymous]
        [Route("Events/Api/Full/{id}")]
        [HttpGet]
        public IActionResult ApiDetailsFull(int id)
        {
            try
            {
                // Static goal names for ultra-fast lookup
                var goalNames = new Dictionary<int, string>
                {
                    {1, "Organizational Building"},
                    {2, "Financial Sustainability"},
                    {3, "Identity/Value Proposition"},
                    {4, "Community Engagement"}
                };

                // Try to get event from database first - single query with no joins
                var eventFromDb = _context.Events
                    .Where(e => e.Id == id)
                    .Select(e => new {
                        e.Id,
                        e.Title,
                        e.Description,
                        e.Type,
                        e.Status,
                        e.DueDate,
                        e.Location,
                        e.Attendees,
                        e.Notes,
                        e.SatisfactionScore,
                        e.StrategicGoalId
                    })
                    .FirstOrDefault();

                if (eventFromDb != null)
                {
                    var goalName = eventFromDb.StrategicGoalId.HasValue && goalNames.ContainsKey(eventFromDb.StrategicGoalId.Value)
                        ? goalNames[eventFromDb.StrategicGoalId.Value]
                        : "Strategic Goal";

                    return Json(new
                    {
                        Id = eventFromDb.Id,
                        Title = eventFromDb.Title,
                        Description = eventFromDb.Description,
                        Type = eventFromDb.Type,
                        Status = eventFromDb.Status,
                        DueDate = eventFromDb.DueDate?.ToString("MMMM dd, yyyy"),
                        Location = eventFromDb.Location,
                        Attendees = eventFromDb.Attendees,
                        Notes = eventFromDb.Notes,
                        SatisfactionScore = eventFromDb.SatisfactionScore,
                        GoalName = goalName,
                        Source = "Database"
                    });
                }

                // Event not found
                return Json(new { error = "Event not found" });
            }
            catch (Exception)
            {
                return Json(new { error = "Failed to load event details" });
            }
        }

        [Authorize(Roles = "Staff")]
        public IActionResult Index()
        {
            var username = User.Identity?.Name ?? string.Empty;
            var items = _events.GetByOwner(username)
                .ToList()
                .Where(e => e.StrategyTemplateId.HasValue && _strategyService.GetStrategy(e.StrategyTemplateId.Value) != null)
                .ToList();

            PopulateEventDisplayData(items);
            return View(items);
        }

        [Authorize(Roles = "Staff")]
        public IActionResult Archived()
        {
            var username = User.Identity?.Name ?? string.Empty;
            var items = _events.GetArchivedByOwner(username)
                .Where(e => e.StrategyTemplateId.HasValue && _strategyService.GetStrategy(e.StrategyTemplateId.Value) != null)
                .ToList();

            PopulateEventDisplayData(items);
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
            eventModel.IsAssignedByAdmin = false; 

            var added = _events.Add(eventModel);

            _activityLog.Log(username, "Created Event", "Event", added.Id, notes: $"Created '{added.Title}'");

            TempData["SuccessMessage"] = "Event created successfully!";
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
            // Preserve assignment/strategy metadata not edited on this form
            eventModel.StrategyTemplateId = existing.StrategyTemplateId;
            eventModel.StrategicGoalId = existing.StrategicGoalId;
            eventModel.StrategyId = existing.StrategyId;
            eventModel.IsAssignedByAdmin = existing.IsAssignedByAdmin;
            eventModel.AssignmentDate = existing.AssignmentDate;
            eventModel.AdminNotes = existing.AdminNotes;

            // Log status changes specifically
            var statusChanged = existing.Status != eventModel.Status;
            var statusChangeNote = statusChanged ? $"Status changed from '{existing.Status}' to '{eventModel.Status}'" : null;

            // Auto-archive when status is Completed
            if (eventModel.Status == "Completed")
            {
                eventModel.IsArchived = true;
                eventModel.CompletionDate = existing.CompletionDate ?? NowEastern;
            }
            else
            {
                eventModel.IsArchived = false;
                eventModel.CompletionDate = null;
            }

            _events.Update(eventModel);

            //Get staff name for logging
            var username = User.Identity?.Name ?? string.Empty;
            // Log the update with status change detail if applicable
            var logAction = statusChanged ? "Changed Event Status" : "Updated Event";
            var logNotes = statusChangeNote ?? $"Updated '{eventModel.Title}'";
            _activityLog.Log(username, logAction, "Event", eventModel.Id, notes: logNotes);

            TempData["SuccessMessage"] = "Event updated successfully!";
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

            var username = User.Identity?.Name ?? string.Empty;
            var eventTitle = eventModel.Title;

            _events.Remove(id);
            _activityLog.Log(username, "Deleted Event", "Event", id, notes: $"Deleted '{eventTitle}'");

            TempData["SuccessMessage"] = "Event deleted successfully!";
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

        private void PopulateEventDisplayData(IEnumerable<Event> events)
        {
            var strategyIds = events
                .Select(e => e.StrategyTemplateId ?? e.StrategyId)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var strategies = _context.Strategies
                .Where(s => strategyIds.Contains(s.Id))
                .Select(s => new { s.Id, s.StrategicGoalId, s.ProgramType })
                .ToList();

            var goalIds = events
                .Where(e => e.StrategicGoalId.HasValue)
                .Select(e => e.StrategicGoalId!.Value)
                .Concat(strategies.Select(s => s.StrategicGoalId))
                .Distinct()
                .ToList();

            var goalNamesByGoalId = _context.StrategicGoals
                .Where(g => goalIds.Contains(g.Id))
                .ToDictionary(g => g.Id, g => g.Name);

            var goalNamesByStrategyId = strategies.ToDictionary(
                s => s.Id,
                s => goalNamesByGoalId.TryGetValue(s.StrategicGoalId, out var goalName)
                    ? goalName
                    : $"Strategic Goal {s.StrategicGoalId}");

            var eventTypesByStrategyId = strategies.ToDictionary(
                s => s.Id,
                s => string.IsNullOrWhiteSpace(s.ProgramType) ? "Community" : s.ProgramType);

            ViewBag.GoalNamesByGoalId = goalNamesByGoalId;
            ViewBag.GoalNamesByStrategyId = goalNamesByStrategyId;
            ViewBag.EventTypesByStrategyId = eventTypesByStrategyId;
        }
    }
}
