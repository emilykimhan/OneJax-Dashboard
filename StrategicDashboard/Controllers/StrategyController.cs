using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

public class StrategyController : Controller
{
    private static List<Strategy> strategies = new()
    {
        new Strategy { Id = 1, Name = "Test Event 1", Description = "Sample description for community engagement", StrategicGoalId = 1, Metrics = new List<Metric>() },
        new Strategy { Id = 2, Name = "Test Event 2", Description = "Another event for leadership growth", StrategicGoalId = 2, Metrics = new List<Metric>() }
    };

    public IActionResult Index(int? goalId)
    {
        var goalStrategies = goalId.HasValue
            ? strategies.Where(s => s.StrategicGoalId == goalId.Value).ToList()
            : strategies.ToList();

        goalStrategies = goalStrategies.OrderByDescending(s => s.Id).ToList();

        ViewBag.GoalId = goalId;
        return View(goalStrategies);
    }

    // Add a new event
    [HttpPost]
    public IActionResult Add(int goalId, string eventName, string eventDescription)
    {
        int newId = strategies.Any() ? strategies.Max(s => s.Id) + 1 : 1;

        strategies.Add(new Strategy
        {
            Id = newId,
            Name = eventName,
            Description = eventDescription,
            StrategicGoalId = goalId,
            Metrics = new List<Metric>()
        });

        return RedirectToAction("Index");
    }

    // Edit an existing event
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

    // Add a metric to an event 
    [HttpPost]
    public IActionResult AddMetric(int strategyId, string description, string target, string progress, string status, string timePeriod)
    {
        var strategy = strategies.FirstOrDefault(s => s.Id == strategyId);
        if (strategy != null)
        {
            strategy.Metrics.Add(new Metric
            {
                Id = strategy.Metrics.Count + 1,
                Description = description,
                Target = target,
                Progress = progress,
                Status = status,
                TimePeriod = timePeriod
            });
        }

        return RedirectToAction("Index", new { goalId = strategy?.StrategicGoalId });
    }
}