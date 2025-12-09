using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OneJaxDashboard.Models;
using System.Collections.Generic;
using System.Linq;
//dina
public class StrategyController : Controller
{
    // Make this public static so StrategyService can access it
    public static List<Strategy> Strategies { get; set; } = new();

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

        var goalStrategies = goalId.HasValue
            ? Strategies.Where(s => s.StrategicGoalId == goalId.Value).ToList()
            : Strategies.ToList();

        goalStrategies = goalStrategies.OrderByDescending(s => s.Id).ToList();

        ViewBag.GoalId = goalId;
        ViewBag.SuccessMessage = TempData["SuccessMessage"]; // For toast display

        return View(goalStrategies);
    }

    [HttpPost]
    public IActionResult Add(int goalId, string eventName, string eventDescription, string? eventDate, string? eventTime)
    {
        int newId = Strategies.Any() ? Strategies.Max(s => s.Id) + 1 : 1;

        var newEvent = new Strategy
        {
            Id = newId,
            Name = eventName,
            Description = eventDescription,
            StrategicGoalId = goalId,
            Date = eventDate,
            Time = eventTime
        };

        Strategies.Add(newEvent);

        string goalName = Goals.FirstOrDefault(g => g.Value == goalId.ToString())?.Text ?? "Unknown Goal";

        TempData["SuccessMessage"] = $"Successfully added event under “{goalName}”";

        return RedirectToAction("Index");
    }

// GET: /Strategy/Edit/5
[HttpGet]
public IActionResult Edit(int id)
{
    // your events are stored in the in-memory `strategies` list
    var evt = strategies.FirstOrDefault(s => s.Id == id);
    if (evt == null)
    {
        return NotFound();
    }

    return View(evt);   // will use Views/Strategy/Edit.cshtml
}

// POST: /Strategy/Edit
[HttpPost]
public IActionResult Edit(int id, string eventName, string eventDescription, int goalId, string? eventDate, string? eventTime)
{
    var evt = strategies.FirstOrDefault(s => s.Id == id);
    if (evt == null)
    {
        return NotFound();
    }

    // Update fields
    evt.Name = eventName;
    evt.Description = eventDescription;
    evt.StrategicGoalId = goalId;
    evt.Date = eventDate;
    evt.Time = eventTime;

    TempData["SuccessMessage"] = "Event updated successfully!";

    // after editing, send them back to View Events
    return RedirectToAction("ViewEvents");
}

    [HttpPost]
    public IActionResult Delete(int id)
    {
        var strategy = Strategies.FirstOrDefault(s => s.Id == id);
        if (strategy != null)
            Strategies.Remove(strategy);

        TempData["SuccessMessage"] = "Event deleted successfully";
        return RedirectToAction("Index");
    }
    public IActionResult ViewEvents()
    {
        // Reuse the in-memory strategies list and the Goals list
        ViewBag.Goals = Goals;

        // All events, newest first
        var allEvents = strategies
            .OrderByDescending(s => s.Id)
            .ToList();

        return View(allEvents);
    }

 
}