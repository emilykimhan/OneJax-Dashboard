using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using OneJaxDashboard.Models;
using OneJaxDashboard.Data;
using OneJaxDashboard.Services;
using System.Collections.Generic;
using System.Linq;
//dina
[Authorize(Roles = "Admin,Staff")]
public class StrategyController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ActivityLogService _activityLog;
    private static readonly DateTime MaxEventDate = new(2030, 12, 31);
    private static readonly string[] DefaultProgramTypes =
    {
        "Humanitarian Awards",
        "Fundraising",
        "Youth",
        "Interfaith",
        "Community",
        "Donor"
    };

    public StrategyController(ApplicationDbContext context, ActivityLogService activityLog)
    {
        _context = context;
        _activityLog = activityLog;
    }

    private static readonly List<SelectListItem> Goals = new()
    {
        new SelectListItem { Value = "1", Text = "Organizational Building" },
        new SelectListItem { Value = "2", Text = "Financial Sustainability" },
        new SelectListItem { Value = "3", Text = "Identity/Value Proposition" },
        new SelectListItem { Value = "4", Text = "Community Engagement" }
    };

    private List<SelectListItem> GetGoalOptions()
    {
        var dbGoals = _context.StrategicGoals
            .Where(g => g.Id >= 1 && g.Id <= 4)
            .OrderBy(g => g.Id)
            .Select(g => new SelectListItem
            {
                Value = g.Id.ToString(),
                Text = g.Name
            })
            .ToList();

        return dbGoals.Count > 0 ? dbGoals : Goals;
    }

    private StrategicGoal? EnsureGoalExists(int goalId)
    {
        var existingGoal = _context.StrategicGoals.FirstOrDefault(g => g.Id == goalId);
        if (existingGoal != null)
        {
            return existingGoal;
        }

        var fallbackGoalName = Goals.FirstOrDefault(g => g.Value == goalId.ToString())?.Text;
        if (string.IsNullOrWhiteSpace(fallbackGoalName))
        {
            return null;
        }

        var newGoal = new StrategicGoal
        {
            Id = goalId,
            Name = fallbackGoalName
        };

        _context.StrategicGoals.Add(newGoal);
        return newGoal;
    }

    private static string ComputeFiscalYear(string? eventDate)
    {
        if (string.IsNullOrWhiteSpace(eventDate) || !DateTime.TryParse(eventDate, out var parsedDate))
        {
            return string.Empty;
        }

        var startYear = parsedDate.Month >= 7 ? parsedDate.Year : parsedDate.Year - 1;
        var endYear = startYear + 1;
        return $"{startYear}/{endYear}";
    }

    private static bool IsPastMaxEventDate(string? eventDate)
    {
        return DateTime.TryParse(eventDate, out var parsedDate) && parsedDate.Date > MaxEventDate;
    }


    public IActionResult Index(int? goalId)
    {
        var programOptions = _context.Programs
            .OrderBy(p => p.ProgramName)
            .ToList();

        var goalOptions = GetGoalOptions();
        ViewBag.Goals = goalOptions;
        ViewBag.Programs = programOptions;
        ViewBag.ProgramTypes = DefaultProgramTypes
            .Concat(programOptions
                .Select(p => p.ProgramType)
                .Where(t => !string.IsNullOrWhiteSpace(t)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList();

        var goalStrategies = goalId.HasValue
            ? _context.Strategies.Where(s => !s.IsArchived && s.StrategicGoalId == goalId.Value).ToList()
            : _context.Strategies.Where(s => !s.IsArchived).ToList();

        goalStrategies = goalStrategies.OrderByDescending(s => s.Id).ToList();

        ViewBag.GoalId = goalId;
        ViewBag.SuccessMessage = TempData["SuccessMessage"];

        return View(goalStrategies);
    }

    [HttpPost]
    public IActionResult Add(int goalId, string? eventName, string eventDescription, string? eventDate, string? eventTime, bool isCrossCollaboration = false, string? partners = null, int? programId = null, string? programType = null)
    {
        if (IsPastMaxEventDate(eventDate))
        {
            TempData["ErrorMessage"] = "Event date cannot be later than 12/31/2030.";
            return RedirectToAction("Index");
        }

        var selectedGoal = EnsureGoalExists(goalId);
        if (selectedGoal == null)
        {
            TempData["ErrorMessage"] = "Please select a valid goal before creating an event.";
            return RedirectToAction("Index");
        }

        var selectedProgram = programId.HasValue
            ? _context.Programs.FirstOrDefault(p => p.Id == programId.Value)
            : null;

        var selectedProgramType = selectedProgram?.ProgramType;
        if (string.IsNullOrWhiteSpace(selectedProgramType) && !string.IsNullOrWhiteSpace(programType))
        {
            selectedProgramType = programType.Trim();
        }

        var resolvedEventName = string.IsNullOrWhiteSpace(eventName)
            ? (selectedProgram?.ProgramName ?? selectedProgramType ?? "Untitled Event")
            : eventName.Trim();

        // Save to database for persistence - only set properties that don't have foreign key constraints
        var dbEvent = new Strategy
        {
            Name = resolvedEventName,
            ProgramId = selectedProgram?.Id,
            ProgramName = selectedProgram?.ProgramName,
            ProgramType = selectedProgramType,
            Description = eventDescription,
            StrategicGoalId = goalId,
            Date = eventDate,
            Time = eventTime,
            CrossCollaboration = isCrossCollaboration ? "Yes" : "No",
            Partners = isCrossCollaboration ? (partners ?? string.Empty).Trim() : string.Empty,
            EventFYear = ComputeFiscalYear(eventDate)
        };

        _context.Strategies.Add(dbEvent);
        _context.SaveChanges();

        string goalName = selectedGoal.Name;
        // Log the creation
        _activityLog.Log("Admin", "Created Core Strategy Event", "Strategy", dbEvent.Id, 
            notes: $"Created strategy event '{eventName}' under {goalName}");
        TempData["SuccessMessage"] = $"Successfully added program under “{goalName}”";

        return RedirectToAction("Index");
    }
    // POST: /Strategy/Edit

    [HttpGet]
    public IActionResult Edit(int id)
    {
        // Fetch the strategy from the database
        var evt = _context.Strategies.FirstOrDefault(s => s.Id == id);
        if (evt == null)
        {
            return NotFound(); // Return 404 if the strategy doesn't exist
        }

        ViewBag.Goals = GetGoalOptions(); // Pass goals for the dropdown
        var programOptions = _context.Programs
            .OrderBy(p => p.ProgramName)
            .ToList();
        ViewBag.Programs = programOptions;
        ViewBag.ProgramTypes = DefaultProgramTypes
            .Concat(programOptions
                .Select(p => p.ProgramType)
                .Where(t => !string.IsNullOrWhiteSpace(t)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t)
            .ToList();
        return View(evt); // Pass the strategy to the view
    }

    [HttpPost]
    public IActionResult Edit(int id, string? eventName, string eventDescription, string? eventDate, string? eventTime, int goalId, bool isCrossCollaboration = false, string? partners = null, int? programId = null, string? programType = null)
    {
        if (IsPastMaxEventDate(eventDate))
        {
            TempData["ErrorMessage"] = "Event date cannot be later than 12/31/2030.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        if (EnsureGoalExists(goalId) == null)
        {
            TempData["ErrorMessage"] = "Please select a valid goal before updating the event.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        // Fetch the strategy from the database
        var evt = _context.Strategies.FirstOrDefault(s => s.Id == id);
        if (evt == null)
        {
            return NotFound(); // Return 404 if the strategy doesn't exist
        }

        var selectedProgram = programId.HasValue
            ? _context.Programs.FirstOrDefault(p => p.Id == programId.Value)
            : null;

        var selectedProgramType = selectedProgram?.ProgramType;
        if (string.IsNullOrWhiteSpace(selectedProgramType) && !string.IsNullOrWhiteSpace(programType))
        {
            selectedProgramType = programType.Trim();
        }

        var resolvedEventName = string.IsNullOrWhiteSpace(eventName)
            ? (selectedProgram?.ProgramName ?? selectedProgramType ?? "Untitled Event")
            : eventName.Trim();

        // Update the strategy's properties
        evt.Name = resolvedEventName;
        evt.ProgramId = selectedProgram?.Id;
        evt.ProgramName = selectedProgram?.ProgramName;
        evt.ProgramType = selectedProgramType;
        evt.CrossCollaboration = isCrossCollaboration ? "Yes" : "No";
        evt.Partners = isCrossCollaboration ? (partners ?? string.Empty).Trim() : string.Empty;
        evt.Description = eventDescription;
        evt.Date = eventDate;
        evt.Time = eventTime;
        evt.StrategicGoalId = goalId;
        evt.EventFYear = ComputeFiscalYear(eventDate);

        // Save changes to the database
        _context.SaveChanges();

        TempData["SuccessMessage"] = "Event updated successfully!";
        return RedirectToAction(nameof(ViewEvents));
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        // Fetch the strategy from the database
        var strategy = _context.Strategies.FirstOrDefault(s => s.Id == id);
        if (strategy == null)
        {
            return NotFound(); // Return 404 if the strategy doesn't exist
        }

        // Remove the strategy from the database
        _context.Strategies.Remove(strategy);
        _context.SaveChanges();

        TempData["SuccessMessage"] = "Event deleted successfully!";
        return RedirectToAction("ViewEvents");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Archive(int id)
    {
        var strategy = _context.Strategies.FirstOrDefault(s => s.Id == id);
        if (strategy == null)
        {
            return NotFound();
        }

        strategy.IsArchived = true;
        strategy.ArchivedAtUtc = DateTime.UtcNow;
        _context.SaveChanges();

        TempData["ProgramsSuccess"] = "Event archived.";
        return RedirectToAction("Archive", "Programs");
    }

    public IActionResult ViewEvents() 
    {
        // Fetch all events from the database
        var events = _context.Strategies
            .Where(s => !s.IsArchived)
            .ToList();
        var hasUpdates = false;
        foreach (var evt in events)
        {
            var computed = ComputeFiscalYear(evt.Date);
            if (!string.Equals(evt.EventFYear ?? string.Empty, computed, StringComparison.Ordinal))
            {
                evt.EventFYear = computed;
                hasUpdates = true;
            }
        }

        if (hasUpdates)
        {
            _context.SaveChanges();
        }

        // ViewEvents should only show the four strategic goals as goal filter tabs.
        ViewBag.Goals = _context.StrategicGoals
            .Where(g => g.Id >= 1 && g.Id <= 4)
            .OrderBy(g => g.Id)
            .Select(g => new SelectListItem
            {
                Value = g.Id.ToString(),
                Text = g.Name
            })
            .ToList();

        var fiscalYears = events
            .Select(e => e.EventFYear)
            .Where(fy => !string.IsNullOrWhiteSpace(fy))
            .Distinct()
            .OrderBy(fy => fy)
            .ToList();

        if (!fiscalYears.Any())
        {
            var now = DateTime.Now;
            var currentStartYear = now.Month >= 7 ? now.Year : now.Year - 1;
            fiscalYears = new List<string>
            {
                $"{currentStartYear - 1}/{currentStartYear}",
                $"{currentStartYear}/{currentStartYear + 1}",
                $"{currentStartYear + 1}/{currentStartYear + 2}"
            };
        }

        ViewBag.FiscalYears = fiscalYears;

        // Pass the events to the view
        return View(events);
    }

}
