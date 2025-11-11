using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OneJax.StrategicDashboard.Models;
using System.Collections.Generic;
using System.Linq;

public class StrategyController : Controller
{
    private static List<Strategy> strategies = new();

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
            ? strategies.Where(s => s.StrategicGoalId == goalId.Value).ToList()
            : strategies.ToList();

        goalStrategies = goalStrategies.OrderByDescending(s => s.Id).ToList();

        ViewBag.GoalId = goalId;
        ViewBag.SuccessMessage = TempData["SuccessMessage"]; // For toast display

        return View(goalStrategies);
    }

    [HttpPost]
    public IActionResult Add(int goalId, string eventName, string eventDescription, string? eventDate, string? eventTime)
    {
        int newId = strategies.Any() ? strategies.Max(s => s.Id) + 1 : 1;

        var newEvent = new Strategy
        {
            Id = newId,
            Name = eventName,
            Description = eventDescription,
            StrategicGoalId = goalId,
            Date = eventDate, 
            Time = eventTime  
        };

        strategies.Add(newEvent);

        var goalName = Goals.FirstOrDefault(g => g.Value == goalId.ToString())?.Text ?? "Selected Goal";

        TempData["SuccessMessage"] = $"Successfully added event under “{goalName}”";

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Edit(int id, string eventName, string eventDescription, int goalId, string? eventDate, string? eventTime)
    {
        var existingEvent = strategies.FirstOrDefault(s => s.Id == id);
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
        var strategy = strategies.FirstOrDefault(s => s.Id == id);
        if (strategy != null)
            strategies.Remove(strategy);

        TempData["SuccessMessage"] = "Event deleted successfully";
        return RedirectToAction("Index");
    }
}