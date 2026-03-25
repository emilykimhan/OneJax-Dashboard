using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OneJaxDashboard.Services;
using OneJaxDashboard.Models;
using OneJaxDashboard.Data;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Drawing.Chart;


namespace OneJaxDashboard.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private const int RecentActivityLimit = 10;
        private readonly ApplicationDbContext _db;
        private readonly EventsService _eventsService;
        private readonly StrategyService _strategyService;
        private readonly ActivityLogService _activityLog;
        private readonly TimeZoneInfo _easternTimeZone;

        private DateTime NowEastern => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _easternTimeZone);

        public AdminController(ApplicationDbContext db, EventsService eventsService, StrategyService strategyService, ActivityLogService activityLog, TimeZoneInfo easternTimeZone)
        {
            _db = db;
            _eventsService = eventsService;
            _strategyService = strategyService;
            _activityLog = activityLog;
            _easternTimeZone = easternTimeZone;
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
                .Where(e => !string.IsNullOrWhiteSpace(e.OwnerUsername))
                .ToList()
                .Where(e => e.StrategyTemplateId.HasValue && _strategyService.GetStrategy(e.StrategyTemplateId.Value) != null)
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
                || entry.Action.StartsWith("Submitted", StringComparison.OrdinalIgnoreCase);
        }

        // GET: /Admin/ArchivedEvents
        public IActionResult ArchivedEvents()
        {
            var archivedEvents = _db.Events
                .Include(e => e.AssignedStaff)
                .Where(e => e.IsArchived)
                .Where(e => !string.IsNullOrWhiteSpace(e.OwnerUsername))
                .ToList()
                .Where(e => e.StrategyTemplateId.HasValue && _strategyService.GetStrategy(e.StrategyTemplateId.Value) != null)
                .ToList();

            return View(archivedEvents);
        }

        // GET: /Admin/AssignEvent
        public IActionResult AssignEvent(int? strategyId)
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
            
            PopulateStaffAndStrategiesDropdown(strategyId);
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

            // Load the strategy template to get the title
            var strategy = eventModel.StrategyTemplateId.HasValue ? _strategyService.GetStrategy(eventModel.StrategyTemplateId.Value) : null;
            if (strategy == null)
            {
                ModelState.AddModelError("", "Please select an event.");
                PopulateStaffAndStrategiesDropdown(null);
                return View(eventModel);
           }

            if (!ModelState.IsValid)
            {
                PopulateStaffAndStrategiesDropdown(null);
                return View(eventModel);
            }

            var staffMember = _db.Staffauth.FirstOrDefault(s => s.Username == selectedStaffUsername);
            var staffName = staffMember?.Name ?? selectedStaffUsername;

            // Set title and related fields from the strategy template
            eventModel.Title = strategy.Name;
            eventModel.StrategicGoalId = strategy.StrategicGoalId;
            eventModel.StrategyId = strategy.Id;

            // Set admin assignment properties
            eventModel.OwnerUsername = selectedStaffUsername;
            eventModel.IsAssignedByAdmin = true;
            eventModel.AssignmentDate = NowEastern;

            var addedEvent = _eventsService.Add(eventModel);
            
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

            PopulateStaffAndStrategiesDropdown(null);
            ViewBag.CurrentStaffUsername = eventModel.OwnerUsername;
            return View(eventModel);
        }

        // POST: /Admin/EditAssignedEvent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAssignedEvent(Event eventModel, string selectedStaffUsername)
        {
            if (string.IsNullOrEmpty(selectedStaffUsername))
            {
                ModelState.AddModelError("", "Please select a staff member.");
            }

            if (!ModelState.IsValid)
            {
                PopulateStaffAndStrategiesDropdown(null);
                ViewBag.CurrentStaffUsername = eventModel.OwnerUsername;
                return View(eventModel);
            }

            var existing = _eventsService.Get(eventModel.Id);
            if (existing == null) return NotFound();

            var staffMember = _db.Staffauth.FirstOrDefault(s => s.Username == selectedStaffUsername);
            var staffName = staffMember?.Name ?? selectedStaffUsername;
            var previousStaffMember = _db.Staffauth.FirstOrDefault(s => s.Username == existing.OwnerUsername);
            var previousStaffName = previousStaffMember?.Name ?? existing.OwnerUsername;
            var previousGoalName = ResolveGoalName(existing.StrategicGoalId);
            var updatedGoalName = ResolveGoalName(eventModel.StrategicGoalId);
            var changeDetails = BuildEventChangeDetails(existing, eventModel, previousStaffName, staffName, previousGoalName, updatedGoalName);

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

        private void PopulateStaffAndStrategiesDropdown(int? selectedStrategyId)
        {
            // Get all staff members
            var staffMembers = _db.Staffauth.ToList();
            ViewBag.StaffMembers = new SelectList(staffMembers, "Username", "Name");

            // Get strategic goals and strategies
            var goals = _strategyService.GetAllStrategicGoals();
            ViewBag.StrategicGoals = new SelectList(goals, "Id", "Name");
            
            // Get all strategies from Core Strategies to use as event templates
            var allStrategies = _strategyService.GetAllStrategies();
            ViewBag.StrategyTemplates = new SelectList(allStrategies, "Id", "Name", selectedStrategyId);
            ViewBag.SelectedStrategyId = selectedStrategyId;
            
            // Load all strategies for display
            ViewBag.Strategies = new SelectList(allStrategies, "Id", "Name");
        }

        private static string BuildEventChangeDetails(
            Event before,
            Event after,
            string previousAssignee,
            string newAssignee,
            string previousGoalName,
            string updatedGoalName)
        {
            var changes = new List<string>();
            AddChange(changes, "Assignee", previousAssignee, newAssignee);
            AddChange(changes, "Strategic Goal", previousGoalName, updatedGoalName);
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

        private string ResolveGoalName(int? goalId)
        {
            if (!goalId.HasValue)
            {
                return "(empty)";
            }

            var goalName = _db.StrategicGoals
                .Where(g => g.Id == goalId.Value)
                .Select(g => g.Name)
                .FirstOrDefault();

            return string.IsNullOrWhiteSpace(goalName) ? $"Goal {goalId.Value}" : goalName;
        }
    }
}
