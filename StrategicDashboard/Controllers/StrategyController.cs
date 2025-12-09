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

    // Clean slate - goals will come from database
    private static readonly List<SelectListItem> Goals = new();

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

    [HttpPost]
    public IActionResult Edit(int id, string eventName, string eventDescription, int goalId, string? eventDate, string? eventTime)
    {
        var existingEvent = Strategies.FirstOrDefault(s => s.Id == id);
        if (existingEvent != null)
        {
            existingEvent.Name = eventName;
            existingEvent.Description = eventDescription;
            existingEvent.StrategicGoalId = goalId;
            existingEvent.Date = eventDate;
            existingEvent.Time = eventTime;
        }

        TempData["SuccessMessage"] = "Event updated successfully";
        return RedirectToAction("Index");
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
}