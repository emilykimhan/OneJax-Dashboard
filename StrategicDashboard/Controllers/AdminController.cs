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
            
            // Get all events (active and completed)
            //var allEvents = _db.Events
            //    .Include(e => e.AssignedStaff)
             //   .Where(e => !e.IsArchived)
            //    .ToList()
            //    .Where(e => e.StrategyTemplateId.HasValue && _strategyService.GetStrategy(e.StrategyTemplateId.Value) != null)
            //    .ToList();

            //var eventCount = allEvents.Count;

            // Get recent activity log entries (last 10)
            var recentActivities = _activityLog.GetAllEntries()
                .OrderByDescending(a => a.Timestamp)
                .Take(10)
                .ToList();

            // Pass data to view
            ViewData["StaffCount"] = staffCount;
           // ViewData["EventCount"] = eventCount;
            ViewData["RecentActivities"] = recentActivities;

            return View();
        }

        // GET: /Admin/ManageEvents
        public IActionResult ManageEvents()
        {
            // Get all non-archived events
            var allEvents = _db.Events
                .Include(e => e.AssignedStaff)
                .Where(e => !e.IsArchived)
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
        public IActionResult ActivityLog()
        {
            var allActivities = _activityLog.GetAllEntries()
                .OrderByDescending(a => a.Timestamp)
                .ToList();

            return View(allActivities);
        }

        // GET: /Admin/ArchivedEvents
        public IActionResult ArchivedEvents()
        {
            var archivedEvents = _db.Events
                .Include(e => e.AssignedStaff)
                .Where(e => e.IsArchived)
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
            eventModel.AssignmentDate = DateTime.Now;

            var addedEvent = _eventsService.Add(eventModel);
            
            // Log the assignment
            var adminUsername = User.Identity?.Name ?? string.Empty;
            _activityLog.Log("Admin", "Assigned Event", "Event", addedEvent.Id, 
                notes: $"Assigned '{addedEvent.Title}' to {staffName}");

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

            var staffMember = _db.Staffauth.FirstOrDefault(s => s.Username == selectedStaffUsername);
            var staffName = staffMember?.Name ?? selectedStaffUsername;

            // Update the event
            eventModel.OwnerUsername = selectedStaffUsername;
            _eventsService.Update(eventModel);

            // Log the update
            var adminUsername = User.Identity?.Name ?? string.Empty;
            _activityLog.Log("Admin", "Updated Assigned Event", "Event", eventModel.Id, 
                notes: $"Updated '{eventModel.Title}' for {staffName}");

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
            var adminUsername = User.Identity?.Name ?? string.Empty;
            _activityLog.Log("Admin", "Deleted Assigned Event", "Event", id, 
                notes: $"Deleted '{eventModel.Title}' assigned to {staffName}");

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
            var adminUsername = User.Identity?.Name ?? string.Empty;
            _activityLog.Log("Admin", "Reactivated Archived Event", "Event", id, 
                notes: $"Reactivated '{eventModel.Title}'");

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
    }
}
