using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

public class StrategyController : Controller
{
    // Temporary in-memory storage for strategies
    private static List<Strategy> strategies = new();

    // Show strategies for a specific goal
    public IActionResult Index(int goalId)
    {
        var goalStrategies = strategies.Where(s => s.StrategicGoalId == goalId).ToList();
        ViewBag.GoalId = goalId;
        return View(goalStrategies);
    }

    // Add a new strategy to a goal
    [HttpPost]
    public IActionResult Add(int goalId, string strategyName)
    {
        strategies.Add(new Strategy
        {
            Id = strategies.Count + 1,
            Name = strategyName,
            StrategicGoalId = goalId,
            Metrics = new List<Metric>()
        });

        Console.WriteLine($"Total strategies: {strategies.Count}");

        return RedirectToAction("Index", new { goalId });
    }

    // Add a metric to a strategy
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