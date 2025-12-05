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
            return View();
        }

        // GET: /Admin/ManageEvents
        public IActionResult ManageEvents()
        {
            var events = _eventsService.GetAll();
            return View(events);
        }

        // GET: /Admin/AssignEvent
        public IActionResult AssignEvent()
        {
            PopulateStaffAndStrategiesDropdown();
            return View(new Event());
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

            if (!ModelState.IsValid)
            {
                PopulateStaffAndStrategiesDropdown();
                return View(eventModel);
            }

            // Set admin assignment properties
            eventModel.OwnerUsername = selectedStaffUsername;
            eventModel.IsAssignedByAdmin = true;
            eventModel.AssignmentDate = DateTime.Now;

            var addedEvent = _eventsService.Add(eventModel);
            
            // Log the assignment
            var adminUsername = User.Identity?.Name ?? string.Empty;
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

            PopulateStaffAndStrategiesDropdown();
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
                PopulateStaffAndStrategiesDropdown();
                ViewBag.CurrentStaffUsername = eventModel.OwnerUsername;
                return View(eventModel);
            }

            // Update the event
            eventModel.OwnerUsername = selectedStaffUsername;
            _eventsService.Update(eventModel);

            // Log the update
            var adminUsername = User.Identity?.Name ?? string.Empty;
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
            var adminUsername = User.Identity?.Name ?? string.Empty;
            _activityLog.Log(adminUsername, "Deleted Assigned Event", "Event", id, 
                notes: $"Deleted '{eventModel.Title}' assigned to {eventModel.OwnerUsername}");

            TempData["SuccessMessage"] = "Event has been deleted.";
            return RedirectToAction("ManageEvents");
        }

        private void PopulateStaffAndStrategiesDropdown()
        {
            // Get all staff members
            var staffMembers = _db.Staffauth.ToList();
            ViewBag.StaffMembers = new SelectList(staffMembers, "Username", "Name");

            // Get strategic goals and strategies
            var goals = _strategyService.GetAllStrategicGoals();
            ViewBag.StrategicGoals = new SelectList(goals, "Id", "Name");
            
            // Empty strategies list - will be populated via JavaScript based on selected goal
            ViewBag.Strategies = new SelectList(Enumerable.Empty<Strategy>(), "Id", "Name");
        }
    }
}
