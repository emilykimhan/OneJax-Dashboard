using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OneJaxDashboard.Services;
using OneJaxDashboard.Models;
using OneJaxDashboard.Data;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Drawing.Chart;
using Microsoft.Extensions.Logging;


namespace OneJaxDashboard.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private const int RecentActivityLimit = 10;
        private const string DashboardSyncOwnerUsername = "staff";
        private readonly ApplicationDbContext _db;
        private readonly EventsService _eventsService;
        private readonly StrategyService _strategyService;
        private readonly ActivityLogService _activityLog;
        private readonly TimeZoneInfo _easternTimeZone;
        private readonly ILogger<AdminController> _logger;

        private DateTime NowEastern => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _easternTimeZone);

        public AdminController(
            ApplicationDbContext db,
            EventsService eventsService,
            StrategyService strategyService,
            ActivityLogService activityLog,
            TimeZoneInfo easternTimeZone,
            ILogger<AdminController> logger)
        {
            _db = db;
            _eventsService = eventsService;
            _strategyService = strategyService;
            _activityLog = activityLog;
            _easternTimeZone = easternTimeZone;
            _logger = logger;
        }

        // GET: /Admin
        public IActionResult Index()
        {
            var staffCount = _db.Staffauth.Count();
            var eventCount = _db.Strategies.Count();

            // Get recent activity log entries (last 10)
            var recentActivities = _activityLog.GetAllEntries()
                .OrderByDescending(a => a.Timestamp)
                .Take(RecentActivityLimit)
                .ToList();

            // Pass data to view
            ViewData["StaffCount"] = staffCount;
            ViewData["EventCount"] = eventCount;
            ViewData["RecentActivities"] = recentActivities;
            ViewData["RecentActivityLimit"] = RecentActivityLimit;

            return View();
        }

        // GET: /Admin/ManageEvents
        public IActionResult ManageEvents()
        {
            // Get all non-archived events
            var allEvents = _db.Events
                .Include(e => e.AssignedStaff)
                .Where(e => !e.IsArchived)
                .Where(e => !string.IsNullOrWhiteSpace(e.OwnerUsername) && e.OwnerUsername != DashboardSyncOwnerUsername)
                .ToList()
                .Where(e => e.StrategyId.HasValue && _strategyService.GetStrategy(e.StrategyId.Value) != null)
                .ToList();

            // Separate into active and completed
            var activeEvents = allEvents.Where(e => e.Status != "Completed").ToList();
            var completedEvents = allEvents.Where(e => e.Status == "Completed").ToList();

            ViewData["ActiveEvents"] = activeEvents;
            ViewData["CompletedEvents"] = completedEvents;

            return View();
        }

        // GET: /Admin/ActivityLog
        public IActionResult ActivityLog(string? filter = "all")
        {
            var normalizedFilter = (filter ?? "all").Trim().ToLowerInvariant();

            var allActivities = _activityLog.GetAllEntries();

            var filteredActivities = normalizedFilter switch
            {
                "events" => allActivities.Where(IsEventActivity),
                "data-entry" => allActivities.Where(IsDataEntryActivity),
                _ => allActivities
            };

            var orderedActivities = filteredActivities
                .OrderByDescending(a => a.Timestamp)
                .ToList();

            ViewData["ActivityFilter"] = normalizedFilter;
            return View(orderedActivities);
        }

        private static bool IsEventActivity(ActivityLogEntry entry)
        {
            var isEventEntity = string.Equals(entry.Entity, "Event", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.Entity, "Strategy", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entry.Entity, "Program", StringComparison.OrdinalIgnoreCase);

            if (!isEventEntity)
            {
                return false;
            }

            return entry.Action.Contains("Event", StringComparison.OrdinalIgnoreCase)
                || entry.Action.Contains("Strategy", StringComparison.OrdinalIgnoreCase)
                || entry.Action.Contains("Program", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsDataEntryActivity(ActivityLogEntry entry)
        {
            var dataEntryEntityTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "DonorEvent",
                "CommunicationRate",
                "FeeForService",
                "Income",
                "BudgetTracking",
                "IdentityAnnualAverage",
                "EventSatisfaction",
                "BoardMemberRecruitment",
                "BoardMeetingAttendance",
                "SelfAssessment",
                "VolunteerProgram",
                "CollabTouch",
                "InterfaithContacts",
                "Diversity",
                "FirstTimeParticipant",
                "InterfaithEvent",
                "YouthAttendance",
                "FaithCommunity",
                "MilestoneAchievement",
                "Demographics",
                "FrameworkPlan2026",
                "SocialMediaEngagement",
                "StaffSurvey",
                "ProfessionalDevelopment",
                "MediaPlacements",
                "WebsiteTraffic"
            };

            if (string.IsNullOrWhiteSpace(entry.Entity) || !dataEntryEntityTypes.Contains(entry.Entity))
            {
                return false;
            }

            return entry.Action.StartsWith("Created", StringComparison.OrdinalIgnoreCase)
                || entry.Action.StartsWith("Updated", StringComparison.OrdinalIgnoreCase)
                || entry.Action.StartsWith("Submitted", StringComparison.OrdinalIgnoreCase)
                || entry.Action.StartsWith("Deleted", StringComparison.OrdinalIgnoreCase);
        }

        // GET: /Admin/ArchivedEvents
        public IActionResult ArchivedEvents()
        {
            var archivedEvents = _db.Events
                .Include(e => e.AssignedStaff)
                .Where(e => e.IsArchived)
                .Where(e => !string.IsNullOrWhiteSpace(e.OwnerUsername) && e.OwnerUsername != DashboardSyncOwnerUsername)
                .ToList()
                .Where(e => e.StrategyId.HasValue && _strategyService.GetStrategy(e.StrategyId.Value) != null)
                .ToList();

            return View(archivedEvents);
        }

        // GET: /Admin/AssignEvent
        public IActionResult AssignEvent(int? strategyId)
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
            PopulateStaffDropdown();
            PopulateStrategyDropdown(strategyId);
            return View(eventModel);
        }

        // POST: /Admin/AssignEvent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignEvent(Event eventModel, string selectedStaffUsername)
        {
            if (string.IsNullOrEmpty(selectedStaffUsername))
            {
                ModelState.AddModelError("", "Please select a staff member to assign this event to.");
            }

            if (!eventModel.StrategyId.HasValue)
            {
                ModelState.AddModelError(nameof(eventModel.StrategyId), "Please select an event.");
                PopulateStaffDropdown();
                PopulateStrategyDropdown(eventModel.StrategyId);
                return View(eventModel);
           }

            if (!ModelState.IsValid)
            {
                PopulateStaffDropdown();
                PopulateStrategyDropdown(eventModel.StrategyId);
                return View(eventModel);
            }

            var strategy = _strategyService.GetStrategy(eventModel.StrategyId.Value);
            if (strategy == null)
            {
                ModelState.AddModelError(nameof(eventModel.StrategyId), "Please select a valid event.");
                PopulateStaffDropdown();
                PopulateStrategyDropdown(eventModel.StrategyId);
                return View(eventModel);
            }

            var staffMember = _db.Staffauth.FirstOrDefault(s => s.Username == selectedStaffUsername);
            if (staffMember == null)
            {
                ModelState.AddModelError(nameof(selectedStaffUsername), "Please select a valid staff member.");
                PopulateStaffDropdown();
                PopulateStrategyDropdown(eventModel.StrategyId);
                return View(eventModel);
            }

            var staffName = staffMember?.Name ?? selectedStaffUsername;

            eventModel.Title = strategy.Name;
            eventModel.Description = strategy.Description;

            // Set admin assignment properties
            eventModel.OwnerUsername = selectedStaffUsername;
            eventModel.IsAssignedByAdmin = true;
            eventModel.AssignmentDate = NowEastern;

            Event addedEvent;
            try
            {
                addedEvent = _eventsService.Add(eventModel);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to assign event in Admin/AssignEvent. StrategyId={StrategyId}, StaffUsername={StaffUsername}",
                    eventModel.StrategyId,
                    selectedStaffUsername);

                ModelState.AddModelError(
                    string.Empty,
                    "Unable to assign event right now. Please refresh the page and try again.");
                PopulateStaffDropdown();
                PopulateStrategyDropdown(eventModel.StrategyId);
                return View(eventModel);
            }
            
            // Log the assignment
            _activityLog.Log("Admin", "Assigned Event", "Event",
                details: $"Id={addedEvent.Id}; Assigned '{addedEvent.Title}' to {staffName}");

            TempData["SuccessMessage"] = $"Event '{eventModel.Title}' has been assigned to {staffName}.";
            return RedirectToAction("ManageEvents");
        }

        // GET: /Admin/EditAssignedEvent/5
        public IActionResult EditAssignedEvent(int id)
        {
            var eventModel = _eventsService.Get(id);
            if (eventModel == null) return NotFound();

            PopulateStaffDropdown();
            PopulateStrategyDropdown(eventModel.StrategyId ?? ResolveStrategyIdByEventTitle(eventModel.Title));
            ViewBag.CurrentStaffUsername = eventModel.OwnerUsername;
            return View(eventModel);
        }

        // POST: /Admin/EditAssignedEvent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAssignedEvent(Event eventModel, string selectedStaffUsername, int? selectedStrategyId)
        {
            if (string.IsNullOrEmpty(selectedStaffUsername))
            {
                ModelState.AddModelError("", "Please select a staff member.");
            }

            if (!selectedStrategyId.HasValue)
            {
                ModelState.AddModelError("selectedStrategyId", "Please select an event.");
            }

            if (!ModelState.IsValid)
            {
                PopulateStaffDropdown();
                PopulateStrategyDropdown(selectedStrategyId);
                ViewBag.CurrentStaffUsername = eventModel.OwnerUsername;
                return View(eventModel);
            }

            var existing = _eventsService.Get(eventModel.Id);
            if (existing == null) return NotFound();

            if (selectedStrategyId.HasValue)
            {
                var selectedStrategy = _db.Strategies.FirstOrDefault(s => s.Id == selectedStrategyId.Value);
                if (selectedStrategy == null)
                {
                    ModelState.AddModelError("selectedStrategyId", "Please select a valid event.");
                    PopulateStaffDropdown();
                    PopulateStrategyDropdown(selectedStrategyId);
                    ViewBag.CurrentStaffUsername = eventModel.OwnerUsername;
                    return View(eventModel);
                }

                eventModel.Title = selectedStrategy.Name;
                eventModel.Description = selectedStrategy.Description;
                eventModel.StrategyId = selectedStrategy.Id;
            }

            var staffMember = _db.Staffauth.FirstOrDefault(s => s.Username == selectedStaffUsername);
            var staffName = staffMember?.Name ?? selectedStaffUsername;
            var previousStaffMember = _db.Staffauth.FirstOrDefault(s => s.Username == existing.OwnerUsername);
            var previousStaffName = previousStaffMember?.Name ?? existing.OwnerUsername;
            var changeDetails = BuildEventChangeDetails(existing, eventModel, previousStaffName, staffName);

            // Update the event
            eventModel.OwnerUsername = selectedStaffUsername;
            _eventsService.Update(eventModel);

            // Log the update
            _activityLog.Log("Admin", "Updated Assigned Event", "Event",
                details: $"Id={eventModel.Id}; Updated '{eventModel.Title}'. Changes: {changeDetails}");

            TempData["SuccessMessage"] = $"Event '{eventModel.Title}' has been updated.";
            return RedirectToAction("ManageEvents");
        }

        // POST: /Admin/DeleteAssignedEvent/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAssignedEvent(int id)
        {
            var eventModel = _eventsService.Get(id);
            if (eventModel == null) return NotFound();

            var staffMember = _db.Staffauth.FirstOrDefault(s => s.Username == eventModel.OwnerUsername);
            var staffName = staffMember?.Name ?? eventModel.OwnerUsername;

            _eventsService.Remove(id);

            // Log the deletion
            _activityLog.Log("Admin", "Deleted Assigned Event", "Event",
                details: $"Id={id}; Deleted '{eventModel.Title}' assigned to {staffName}");

            TempData["SuccessMessage"] = "Event has been deleted.";
            return RedirectToAction("ManageEvents");
        }

        // POST: /Admin/UnarchiveEvent/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UnarchiveEvent(int id)
        {
            var eventModel = _eventsService.Get(id);
            if (eventModel == null) return NotFound();

            _eventsService.Unarchive(id);

            // Log the unarchive action
            _activityLog.Log("Admin", "Reactivated Archived Event", "Event",
                details: $"Id={id}; Reactivated '{eventModel.Title}'");

            TempData["SuccessMessage"] = $"Event '{eventModel.Title}' has been reactivated.";
            return RedirectToAction("ArchivedEvents");
        }

        private void PopulateStaffDropdown()
        {
            var staffMembers = _db.Staffauth
                .Where(s => !string.IsNullOrWhiteSpace(s.Username))
                .OrderBy(s => s.Name)
                .ThenBy(s => s.Username)
                .Select(s => new
                {
                    s.Username,
                    DisplayName = string.IsNullOrWhiteSpace(s.Name) ? s.Username : s.Name
                })
                .ToList();

            ViewBag.StaffMembers = new SelectList(staffMembers, "Username", "DisplayName");
        }

        private void PopulateStrategyDropdown(int? selectedStrategyId)
        {
            try
            {
                var strategies = _db.Strategies
                    .AsNoTracking()
                    .Select(s => new
                    {
                        s.Id,
                        Name = string.IsNullOrWhiteSpace(s.Name) ? $"Untitled Event #{s.Id}" : s.Name
                    })
                    .OrderBy(s => s.Name)
                    .ToList();

                ViewBag.StrategyTemplates = new SelectList(strategies, "Id", "Name", selectedStrategyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load strategies for Admin/AssignEvent dropdown.");
                ViewBag.StrategyTemplates = new SelectList(Enumerable.Empty<object>(), "Id", "Name", selectedStrategyId);
                TempData["ErrorMessage"] = "Unable to load events list. Please contact support if this persists.";
            }
        }

        private int? ResolveStrategyIdByEventTitle(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            return _db.Strategies
                .Where(s => s.Name == title)
                .Select(s => (int?)s.Id)
                .FirstOrDefault();
        }

        private static string BuildEventChangeDetails(
            Event before,
            Event after,
            string previousAssignee,
            string newAssignee)
        {
            var changes = new List<string>();
            AddChange(changes, "Assignee", previousAssignee, newAssignee);
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
            AddChange(changes, "Admin Notes", before.AdminNotes, after.AdminNotes);

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

    }
}
