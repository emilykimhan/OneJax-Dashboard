using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OneJax.StrategicDashboard.Models;
using System.Collections.Generic;
using System.Linq;

public class StrategyController : Controller
{
    // In-memory "database" (no hardcoded initial events)
    private static List<Strategy> strategies = new();

    public IActionResult Index(int? goalId)
    {
        // Always populate the goals list
        ViewBag.Goals = new List<SelectListItem>
        {
            new SelectListItem { Value = "1", Text = "Community Engagement" },
            new SelectListItem { Value = "2", Text = "Education & Awareness" },
            new SelectListItem { Value = "3", Text = "Leadership Development" },
            new SelectListItem { Value = "4", Text = "Advocacy & Policy" }
        };

        // Only show events filtered by goal if requested
        var goalStrategies = goalId.HasValue
            ? strategies.Where(s => s.StrategicGoalId == goalId.Value).ToList()
            : strategies.ToList();

        goalStrategies = goalStrategies.OrderByDescending(s => s.Id).ToList();

        ViewBag.GoalId = goalId;
        return View(goalStrategies);
    }

    [HttpPost]
    public IActionResult Add(int goalId, string eventName, string eventDescription)
    {
        int newId = strategies.Any() ? strategies.Max(s => s.Id) + 1 : 1;

        strategies.Add(new Strategy
        {
            Id = newId,
            Name = eventName,
            Description = eventDescription,
            StrategicGoalId = goalId
        });

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Edit(int id, string eventName, string eventDescription, int goalId)
    {
        var existingEvent = strategies.FirstOrDefault(s => s.Id == id);
        if (existingEvent != null)
        {
            existingEvent.Name = eventName;
            existingEvent.Description = eventDescription;
            existingEvent.StrategicGoalId = goalId;
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        var strategy = strategies.FirstOrDefault(s => s.Id == id);
        if (strategy != null)
            strategies.Remove(strategy);

        return RedirectToAction("Index");
    }
}