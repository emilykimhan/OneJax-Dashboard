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
                    var goalName = ResolveGoalNameForEvent(eventFromDb);
                    ViewBag.GoalName = string.IsNullOrWhiteSpace(goalName) ? "Strategic Goal" : goalName;

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
                        e.StrategyId,
                        e.Title,
                        e.Description,
                        e.Type,
                        e.Status,
                        e.DueDate,
                        e.Location,
                        e.Attendees,
                        e.Notes,
                        e.SatisfactionScore
                    })
                    .FirstOrDefault();

                if (eventFromDb == null)
                {
                    return Json(new { error = "Event not found" });
                }

                var goalName = eventFromDb.StrategyId.HasValue && goalNames.TryGetValue(
                    _context.Strategies
                        .Where(s => s.Id == eventFromDb.StrategyId.Value)
                        .Select(s => s.StrategicGoalId)
                        .FirstOrDefault(),
                    out var resolvedName)
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
                        e.StrategyId,
                        e.Title,
                        e.Description,
                        e.Type,
                        e.Status,
                        e.DueDate,
                        e.Location,
                        e.Attendees,
                        e.Notes,
                        e.SatisfactionScore
                    })
                    .FirstOrDefault();

                if (eventFromDb != null)
                {
                    var goalName = eventFromDb.StrategyId.HasValue
                        ? _context.Strategies
                            .Where(s => s.Id == eventFromDb.StrategyId.Value)
                            .Select(s => s.StrategicGoal != null ? s.StrategicGoal.Name : null)
                            .FirstOrDefault()
                        : "Strategic Goal";

                    goalName = string.IsNullOrWhiteSpace(goalName) ? "Strategic Goal" : goalName;

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
                .ToList();

            BackfillMissingStrategyLinks(items);
            PopulateEventDisplayData(items);
            return View(items);
        }

        [Authorize(Roles = "Staff")]
        public IActionResult Archived()
        {
            var username = User.Identity?.Name ?? string.Empty;
            var items = _events.GetArchivedByOwner(username)
                .ToList();

            BackfillMissingStrategyLinks(items);
            PopulateEventDisplayData(items);
            return View(items);
        }

        [Authorize(Roles = "Staff")]
        public IActionResult Create(int? strategyId)
        {
            Event eventModel = new Event();
            if (strategyId.HasValue)
            {
                var strategy = _strategyService.GetStrategy(strategyId.Value);
                if (strategy != null)
                {
                    eventModel.StrategyId = strategy.Id;
                    eventModel.Title = strategy.Name;
                    eventModel.Description = strategy.Description;
                }
            }
            PopulateStrategyDropdown(strategyId);
            return View(eventModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Staff")]
        public IActionResult Create(Event eventModel)
        {
            if (!eventModel.StrategyId.HasValue)
            {
                ModelState.AddModelError(nameof(eventModel.StrategyId), "Please select an event.");
                PopulateStrategyDropdown(eventModel.StrategyId);
                return View(eventModel);
            }

            if (!ModelState.IsValid)
            {
                PopulateStrategyDropdown(eventModel.StrategyId);
                return View(eventModel);
            }

            var strategy = _strategyService.GetStrategy(eventModel.StrategyId.Value);
            if (strategy == null)
            {
                ModelState.AddModelError(nameof(eventModel.StrategyId), "Please select a valid event.");
                PopulateStrategyDropdown(eventModel.StrategyId);
                return View(eventModel);
            }

            eventModel.Title = strategy.Name;
            eventModel.Description = strategy.Description;
            eventModel.StrategyId = strategy.Id;
            if (string.IsNullOrWhiteSpace(eventModel.Type))
            {
                eventModel.Type = strategy.ProgramType ?? string.Empty;
            }

            var username = User.Identity?.Name ?? string.Empty;
            eventModel.OwnerUsername = username;
            eventModel.IsAssignedByAdmin = false; 

            var added = _events.Add(eventModel);

            _activityLog.Log(username, "Created Event", "Event", details: $"Id={added.Id}; Created '{added.Title}'");

            TempData["SuccessMessage"] = "Event created successfully!";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Staff")]
        public IActionResult Edit(int id)
        {
            var eventModel = _events.Get(id);
            if (eventModel == null) return NotFound();
            if (!IsOwner(eventModel)) return Forbid();

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
                return View(eventModel);
            }

            // Keep owner unchanged
            eventModel.OwnerUsername = existing.OwnerUsername;
            // Keep event title unchanged once assigned
            eventModel.Title = existing.Title;
            eventModel.StrategyId = existing.StrategyId;
            eventModel.IsAssignedByAdmin = existing.IsAssignedByAdmin;
            eventModel.AssignmentDate = existing.AssignmentDate;
            eventModel.AdminNotes = existing.AdminNotes;

            var changeDetails = BuildEventChangeDetails(existing, eventModel);
            var statusChanged = !string.Equals(existing.Status ?? string.Empty, eventModel.Status ?? string.Empty, StringComparison.OrdinalIgnoreCase);

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
            var logAction = statusChanged ? "Changed Event Status" : "Updated Event";
            var logNotes = $"Updated '{eventModel.Title}'. Changes: {changeDetails}";
            _activityLog.Log(username, logAction, "Event", details: $"Id={eventModel.Id}; {logNotes}");

            TempData["SuccessMessage"] = "Event updated successfully!";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Staff")]
        public IActionResult Delete(int id)
        {
            var eventModel = _events.Get(id);
            if (eventModel == null) return NotFound();
            if (!IsOwner(eventModel)) return Forbid();
            PopulateEventDisplayData(new[] { eventModel });
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
            _activityLog.Log(username, "Deleted Event", "Event", details: $"Id={id}; Deleted '{eventTitle}'");

            TempData["SuccessMessage"] = "Event deleted successfully!";
            return RedirectToAction("Index");
        }

        private bool IsOwner(Event e)
        {
            var username = User.Identity?.Name ?? string.Empty;
            return string.Equals(e.OwnerUsername, username, StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildEventChangeDetails(Event before, Event after)
        {
            var changes = new List<string>();
            AddChange(changes, "Description", before.Description, after.Description);
            AddChange(changes, "Type", before.Type, after.Type);
            AddChange(changes, "Location", before.Location, after.Location);
            AddChange(changes, "Status", before.Status, after.Status);
            AddDateChange(changes, "Start Date", before.StartDate, after.StartDate);
            AddDateChange(changes, "End Date", before.EndDate, after.EndDate);
            AddDateChange(changes, "Due Date", before.DueDate, after.DueDate);
            AddNumberChange(changes, "Attendees", before.Attendees, after.Attendees);
            AddDecimalChange(changes, "Satisfaction Score", before.SatisfactionScore, after.SatisfactionScore);
            AddChange(changes, "Notes", before.Notes, after.Notes);
            AddChange(changes, "Pre Assessment Data", before.PreAssessmentData, after.PreAssessmentData);
            AddChange(changes, "Post Assessment Data", before.PostAssessmentData, after.PostAssessmentData);

            return changes.Count > 0 ? string.Join("; ", changes) : "No field changes detected";
        }

        private static void AddChange(List<string> changes, string fieldName, string? before, string? after)
        {
            var oldValue = Normalize(before);
            var newValue = Normalize(after);
            if (string.Equals(oldValue, newValue, StringComparison.Ordinal))
            {
                return;
            }

            changes.Add($"{fieldName}: '{Display(oldValue)}' -> '{Display(newValue)}'");
        }

        private static void AddDateChange(List<string> changes, string fieldName, DateTime? before, DateTime? after)
        {
            var oldValue = before?.Date;
            var newValue = after?.Date;
            if (oldValue == newValue)
            {
                return;
            }

            changes.Add($"{fieldName}: '{DisplayDate(oldValue)}' -> '{DisplayDate(newValue)}'");
        }

        private static void AddNumberChange(List<string> changes, string fieldName, int before, int after)
        {
            if (before == after)
            {
                return;
            }

            changes.Add($"{fieldName}: '{before}' -> '{after}'");
        }

        private static void AddDecimalChange(List<string> changes, string fieldName, decimal? before, decimal? after)
        {
            if (before == after)
            {
                return;
            }

            changes.Add($"{fieldName}: '{DisplayDecimal(before)}' -> '{DisplayDecimal(after)}'");
        }

        private static string Normalize(string? value) => (value ?? string.Empty).Trim();
        private static string Display(string value) => string.IsNullOrEmpty(value) ? "(empty)" : value;
        private static string DisplayDate(DateTime? value) => value.HasValue ? value.Value.ToString("yyyy-MM-dd") : "(empty)";
        private static string DisplayDecimal(decimal? value) => value.HasValue ? value.Value.ToString("0.##") : "(empty)";

        private void PopulateEventDisplayData(IEnumerable<Event> events)
        {
            var eventList = events.ToList();
            var strategyIds = eventList
                .Select(e => e.StrategyId)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .ToList();

            var strategies = _context.Strategies
                .Where(s => strategyIds.Contains(s.Id))
                .Select(s => new { s.Id, s.StrategicGoalId, s.ProgramType })
                .ToList();

            var goalIds = strategies
                .Select(s => s.StrategicGoalId)
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

            ViewBag.GoalNamesByStrategyId = goalNamesByStrategyId;
            ViewBag.EventTypesByStrategyId = eventTypesByStrategyId;
        }

        private void BackfillMissingStrategyLinks(List<Event> events)
        {
            var orphanedEvents = events
                .Where(e => !e.StrategyId.HasValue && !string.IsNullOrWhiteSpace(e.Title))
                .ToList();

            if (!orphanedEvents.Any())
            {
                return;
            }

            var candidateTitles = orphanedEvents
                .Select(e => e.Title.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var strategyMatches = _context.Strategies
                .Where(s => candidateTitles.Contains(s.Name))
                .Select(s => new { s.Id, s.Name, s.ProgramType })
                .ToList();

            if (!strategyMatches.Any())
            {
                return;
            }

            var strategiesByName = strategyMatches
                .GroupBy(s => s.Name, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var hasChanges = false;
            foreach (var evt in orphanedEvents)
            {
                var title = evt.Title.Trim();
                if (!strategiesByName.TryGetValue(title, out var matchedStrategy))
                {
                    continue;
                }

                evt.StrategyId = matchedStrategy.Id;
                if (string.IsNullOrWhiteSpace(evt.Type))
                {
                    evt.Type = matchedStrategy.ProgramType ?? string.Empty;
                }

                hasChanges = true;
            }

            if (hasChanges)
            {
                _context.SaveChanges();
            }
        }

        private string ResolveGoalNameForEvent(Event evt)
        {
            if (!evt.StrategyId.HasValue)
            {
                return "Strategic Goal";
            }

            var goalName = _context.Strategies
                .Where(s => s.Id == evt.StrategyId.Value)
                .Select(s => s.StrategicGoal != null ? s.StrategicGoal.Name : null)
                .FirstOrDefault();

            return string.IsNullOrWhiteSpace(goalName) ? "Strategic Goal" : goalName;
        }

        private void PopulateStrategyDropdown(int? selectedStrategyId)
        {
            var allStrategies = _strategyService.GetAllStrategies();
            ViewBag.StrategyTemplates = new SelectList(allStrategies, "Id", "Name", selectedStrategyId);
        }
    }
}
