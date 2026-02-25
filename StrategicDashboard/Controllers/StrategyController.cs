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

        // Ensure existing strategies have a default EventType
        var strategies = _context.Strategies.Where(s => string.IsNullOrEmpty(s.EventType)).ToList();
        foreach (var strategy in strategies)
        {
            strategy.EventType = "Community";
        }
        _context.SaveChanges();

        var goalStrategies = goalId.HasValue
            ? _context.Strategies.Where(s => s.StrategicGoalId == goalId.Value).ToList()
            : _context.Strategies.ToList();

        goalStrategies = goalStrategies.OrderByDescending(s => s.Id).ToList();

        ViewBag.GoalId = goalId;
        ViewBag.SuccessMessage = TempData["SuccessMessage"];

        return View(goalStrategies);
    }

    [HttpPost]
    public IActionResult Add(int goalId, string eventName, string eventDescription, string? eventDate, string? eventTime, string eventType = "Community", string? fiscalYear = null, string? programName = null)
    {
        int newId = Strategies.Any() ? Strategies.Max(s => s.Id) + 1 : 1;

        // Save to database for persistence - only set properties that don't have foreign key constraints
        var dbEvent = new Strategy
        {
            Name = string.IsNullOrWhiteSpace(eventName) ? (programName ?? string.Empty) : eventName,
            ProgramName = programName ?? string.Empty,
            Description = eventDescription,
            StrategicGoalId = goalId,
            Date = eventDate,
            Time = eventTime,
            EventType = eventType,
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
        return View(evt); // Pass the strategy to the view
    }

    [HttpPost]
    public IActionResult Edit(int id, string eventName, string eventDescription, string eventDate, string eventTime, int goalId, string eventType, string? fiscalYear, string? programName=null)
    {
        // Fetch the strategy from the database
        var evt = _context.Strategies.FirstOrDefault(s => s.Id == id);
        if (evt == null)
        {
            return NotFound(); // Return 404 if the strategy doesn't exist
        }

        // Update the strategy's properties
        evt.Name = string.IsNullOrWhiteSpace(eventName) ? (programName ?? string.Empty) : eventName;
        evt.ProgramName = programName ?? string.Empty;
        evt.EventType = eventType;
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