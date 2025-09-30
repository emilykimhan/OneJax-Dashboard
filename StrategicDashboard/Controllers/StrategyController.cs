using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

public class StrategyController : Controller
{
    // Temporary in-memory storage for strategies
    private static List<Strategy> strategies = new();

    // Show strategies for a specific goal
    public IActionResult Index(string goalName)
    {
        var goalStrategies = strategies.Where(s => s.GoalName == goalName).ToList();
        ViewBag.GoalName = goalName;
        return View(goalStrategies);
    }

    // Add a new strategy to a goal
    [HttpPost]
    public IActionResult Add(string goalName, string strategyName)
    {
        strategies.Add(new Strategy
        {
            Id = strategies.Count + 1,
            Name = strategyName,
            GoalName = goalName
        });
        return RedirectToAction("Index", new { goalName });
    }
}