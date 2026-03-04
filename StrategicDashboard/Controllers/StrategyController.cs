using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OneJaxDashboard.Models;
using OneJaxDashboard.Data;
using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml.Features;
//dina
public class StrategyController : Controller
{
    private readonly ApplicationDbContext _context;

    // Keep the static list for backward compatibility, but also save to database
    public static List<Strategy> Strategies { get; set; } = new();

    public StrategyController(ApplicationDbContext context)
    {
        _context = context;
    }

    private static readonly List<SelectListItem> Goals = new()
    {
        new SelectListItem { Value = "1", Text = "Organizational Building" },
        new SelectListItem { Value = "2", Text = "Financial Sustainability" },
        new SelectListItem { Value = "3", Text = "Identity/Value Proposition" },
        new SelectListItem { Value = "4", Text = "Community Engagement" }
    };


    public IActionResult Index(int? goalId)
    {
        ViewBag.Goals = Goals;
        ViewBag.Programs = _context.Programs
            .OrderBy(p => p.ProgramName)
            .ToList();

        var goalStrategies = goalId.HasValue
            ? _context.Strategies.Where(s => s.StrategicGoalId == goalId.Value).ToList()
            : _context.Strategies.ToList();

        goalStrategies = goalStrategies.OrderByDescending(s => s.Id).ToList();

        ViewBag.GoalId = goalId;
        ViewBag.SuccessMessage = TempData["SuccessMessage"];

        return View(goalStrategies);
    }

    [HttpPost]
    public IActionResult Add(int goalId, string eventName, string eventDescription, string? eventDate, string? eventTime, bool isCrossCollaboration = false, string? partners = null, string? fiscalYear = null, int? programId = null)
    {
        var selectedProgram = programId.HasValue
            ? _context.Programs.FirstOrDefault(p => p.Id == programId.Value)
            : null;

        // Save to database for persistence - only set properties that don't have foreign key constraints
        var dbEvent = new Strategy
        {
            Name = string.IsNullOrWhiteSpace(eventName) ? (selectedProgram?.ProgramName ?? string.Empty) : eventName,
            ProgramId = selectedProgram?.Id,
            ProgramName = selectedProgram?.ProgramName ?? string.Empty,
            Description = eventDescription,
            StrategicGoalId = goalId,
            Date = eventDate,
            Time = eventTime,
            CrossCollaboration = isCrossCollaboration ? "Yes" : "No",
            Partners = isCrossCollaboration ? (partners ?? string.Empty).Trim() : string.Empty,
            EventFYear = fiscalYear ?? string.Empty
        };
        // Also persist to FiscalYear if the model has that property (keeps older/newer versions in sync)
        var fyProp = dbEvent.GetType().GetProperty("FiscalYear");
        if (fyProp != null)
        {
            fyProp.SetValue(dbEvent, fiscalYear ?? string.Empty);
        }

        _context.Strategies.Add(dbEvent);
        _context.SaveChanges();

        string goalName = Goals.FirstOrDefault(g => g.Value == goalId.ToString())?.Text ?? "Unknown Goal";

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

        ViewBag.Goals = Goals; // Pass goals for the dropdown
        ViewBag.Programs = _context.Programs
            .OrderBy(p => p.ProgramName)
            .ToList();
        return View(evt); // Pass the strategy to the view
    }

    [HttpPost]
    public IActionResult Edit(int id, string eventName, string eventDescription, string eventDate, string eventTime, int goalId, bool isCrossCollaboration = false, string? partners = null, string? fiscalYear = null, int? programId = null)
    {
        // Fetch the strategy from the database
        var evt = _context.Strategies.FirstOrDefault(s => s.Id == id);
        if (evt == null)
        {
            return NotFound(); // Return 404 if the strategy doesn't exist
        }

        var selectedProgram = programId.HasValue
            ? _context.Programs.FirstOrDefault(p => p.Id == programId.Value)
            : null;

        // Update the strategy's properties
        evt.Name = string.IsNullOrWhiteSpace(eventName) ? (selectedProgram?.ProgramName ?? string.Empty) : eventName;
        evt.ProgramId = selectedProgram?.Id;
        evt.ProgramName = selectedProgram?.ProgramName ?? string.Empty;
        evt.CrossCollaboration = isCrossCollaboration ? "Yes" : "No";
        evt.Partners = isCrossCollaboration ? (partners ?? string.Empty).Trim() : string.Empty;
        evt.Description = eventDescription;
        evt.Date = eventDate;
        evt.Time = eventTime;
        evt.StrategicGoalId = goalId;
        evt.EventFYear = fiscalYear ?? string.Empty;
        // Also persist to FiscalYear if the model has that property (keeps older/newer versions in sync)
        var fyProp = evt.GetType().GetProperty("FiscalYear");
        if (fyProp != null)
        {
            fyProp.SetValue(evt, fiscalYear ?? string.Empty);
        }

        // Save changes to the database
        _context.SaveChanges();

        TempData["SuccessMessage"] = "Program updated successfully!";
        return RedirectToAction("Index");
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

        TempData["SuccessMessage"] = "Program deleted successfully!";
        return RedirectToAction("ViewEvents");
    }
    public IActionResult ViewEvents() 
    {
        // Fetch all events from the database
        var events = _context.Strategies.ToList();

        ViewBag.Goals = _context.StrategicGoals
        .Select(g => new SelectListItem
        {
            Value = g.Id.ToString(),
            Text = g.Name
        })
        .ToList();

        ViewBag.FiscalYears = new List<string> { "2025/2026", "2026/2027", "2027/2028" };

        // Pass the events to the view
        return View(events);
    }

}
