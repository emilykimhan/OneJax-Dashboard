using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OneJaxDashboard.Services;
using OneJaxDashboard.Models;
using OneJaxDashboard.Data;


namespace OneJaxDashboard.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly EventsService _eventsService;
        private readonly StrategyService _strategyService;
        private readonly ActivityLogService _activityLog;

        public AdminController(ApplicationDbContext db, EventsService eventsService, StrategyService strategyService, ActivityLogService activityLog)
        {
            _db = db;
            _eventsService = eventsService;
            _strategyService = strategyService;
            _activityLog = activityLog;
        }

        // GET: /Admin
        public IActionResult Index()
        {
            var staffCount = _db.Staffauth.Count();
            var totalEvents = _eventsService.GetAll().Count();
            var assignedEvents = _eventsService.GetAll().Count(e => e.IsAssignedByAdmin);

            ViewData["StaffCount"] = staffCount;
            ViewData["TotalEvents"] = totalEvents;
            ViewData["AssignedEvents"] = assignedEvents;

            // Get all activity log entries from all staff
            var allEntries = _activityLog.GetAllEntries().OrderByDescending(e => e.Timestamp).Take(50).ToList();
            var activityLogData = allEntries
                .Select(e => (e.Username, e.Action, e.EntityType, e.EntityId, e.Timestamp, e.Notes))
                .ToList();
            ViewData["ActivityLog"] = activityLogData;

            //Get Data Entry specific activites for separate display 
            var dataEntryActivities = allEntries
                .Where(e => e.EntityType != null && (
                    e.EntityType.Contains("MediaPlacement", StringComparison.OrdinalIgnoreCase) ||
                    e.EntityType.Contains("WebsiteTraffic", StringComparison.OrdinalIgnoreCase) ||
                    e.EntityType.Contains("ProfessionalDevelopment", StringComparison.OrdinalIgnoreCase) ||
                    e.EntityType.Contains("StaffSurvey", StringComparison.OrdinalIgnoreCase) ||
                    e.EntityType.Contains("Youth") ||
                    e.EntityType.Contains("Interfaith") ||
                    e.EntityType.Contains("Community") ||
                    e.EntityType.Contains("Donor") ||
                    e.EntityType.Contains("Identity") ||
                    e.EntityType.Contains("Financial") ||
                    e.EntityType.Contains("Organization")
                ))
                .Take(30)
                .Select(e => (e.Username, e.Action, e.EntityType, e.EntityId, e.Timestamp, e.Notes))
                .ToList();
            ViewData["DataEntryActivities"] = dataEntryActivities;

            //Count dataentry activites by type for statistics 
            var dataEntryStats = new Dictionary<string, int>
            {
                 ["MediaPlacements"] = allEntries.Count(e => e.EntityType != null && e.EntityType.Equals("MediaPlacement", StringComparison.OrdinalIgnoreCase)),
                ["WebsiteTraffic"] = allEntries.Count(e => e.EntityType != null && e.EntityType.Equals("WebsiteTraffic", StringComparison.OrdinalIgnoreCase)),
                ["ProfessionalDevelopment"] = allEntries.Count(e => e.EntityType != null && e.EntityType.Equals("ProfessionalDevelopment", StringComparison.OrdinalIgnoreCase)),
                ["StaffSurveys"] = allEntries.Count(e => e.EntityType != null && e.EntityType.Equals("StaffSurvey", StringComparison.OrdinalIgnoreCase)),
                ["Other"] = allEntries.Count(e => e.EntityType != null && 
                    (e.EntityType.Contains("Youth") || e.EntityType.Contains("Interfaith") || 
                     e.EntityType.Contains("Community") || e.EntityType.Contains("Donor") ||
                     e.EntityType.Contains("Identity") || e.EntityType.Contains("Financial") ||
                     e.EntityType.Contains("Organization")))
            };
            ViewData["DataEntryStats"] = dataEntryStats;

            // Get event tracking data - recent activity on assigned events (only active ones)
            var assignedEventsList = _eventsService.GetAll().Where(e => e.IsAssignedByAdmin).ToList();
            var eventTrackingData = new List<(int EventId, string EventTitle, string CurrentStatus, List<(string Action, string? Notes, DateTime Timestamp)> History)>();
            
            foreach (var evt in assignedEventsList)
            {
                var eventActivityEntries = _activityLog.GetEntriesByEntityId("Event", evt.Id)
                    .OrderByDescending(e => e.Timestamp)
                    .ToList();
                
                var history = eventActivityEntries
                    .Select(e => (e.Action, e.Notes, e.Timestamp))
                    .ToList();
                
                eventTrackingData.Add((
                    evt.Id,
                    evt.Title,
                    evt.Status,
                    history
                ));
            }
            
            ViewData["EventTrackingLog"] = eventTrackingData;

            return View();
        }

        // GET: /Admin/ManageEvents
        public IActionResult ManageEvents()
        {
            var activeEvents = _eventsService.GetAll()
                .Where(e => e.StrategyTemplateId.HasValue && _strategyService.GetStrategy(e.StrategyTemplateId.Value) != null)
                .ToList();
            return View(activeEvents);
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

            // Set title and related fields from the strategy template
            eventModel.Title = strategy.Name;
            eventModel.StrategicGoalId = strategy.StrategicGoalId;
            eventModel.StrategyId = strategy.Id;

            // Set admin assignment properties
            eventModel.OwnerUsername = selectedStaffUsername;
            eventModel.IsAssignedByAdmin = true;
            eventModel.AssignmentDate = DateTime.Now;

            var addedEvent = _eventsService.Add(eventModel);
            
            // Log the assignment
            var adminUsername = User.Identity?.Name ?? "Admin";
            _activityLog.Log(adminUsername, "Assigned Event", "Event", addedEvent.Id, 
                notes: $"Assigned '{addedEvent.Title}' to {selectedStaffUsername}");

            TempData["SuccessMessage"] = $"Event '{eventModel.Title}' has been assigned to {selectedStaffUsername}.";
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

            // Update the event
            eventModel.OwnerUsername = selectedStaffUsername;
            _eventsService.Update(eventModel);

            // Log the update
            var adminUsername = User.Identity?.Name ?? "Admin";
            _activityLog.Log(adminUsername, "Updated Assigned Event", "Event", eventModel.Id, 
                notes: $"Updated '{eventModel.Title}' for {selectedStaffUsername}");

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

            _eventsService.Remove(id);

            // Log the deletion
            var adminUsername = User.Identity?.Name ?? "Admin";
            _activityLog.Log(adminUsername, "Deleted Assigned Event", "Event", id, 
                notes: $"Deleted '{eventModel.Title}' assigned to {eventModel.OwnerUsername}");

            TempData["SuccessMessage"] = "Event has been deleted.";
            return RedirectToAction("ManageEvents");
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
    }
}

