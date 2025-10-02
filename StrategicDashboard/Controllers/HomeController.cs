using Microsoft.AspNetCore.Mvc;
using OneJax.StrategicDashboard.Models;
using System.Collections.Generic;
using System.Linq;

public class HomeController : Controller
{
    public IActionResult Index(string status, string time, string goal)
    {
        // Sample data; supposed to be a herarchical structure of goals, strategies, and metrics
        var allGoals = new List<StrategicGoal>
        {
            new StrategicGoal
            {
                Id = 1,
                Name = "Community Engagement",
                Strategies = new List<Strategy>
                {
                    new Strategy
                    {
                        Id = 1,
                        Name = "Identify and cultivate partnerships",
                        Metrics = new List<Metric>
                        {
                            new Metric { Id = 1, Description = "Launch joint initiatives", Target = "3 initiatives, 85% satisfaction", Progress = "2 initiatives, 80% satisfaction", Status = "Active", TimePeriod = "Monthly" },
                            new Metric { Id = 2, Description = "Increase collaborations", Target = "3 to 10 by FY 26-27", Progress = "5 collaborations", Status = "Upcoming", TimePeriod = "Yearly" }
                        }
                    }
                }
            },
            // Add more goals, strategies, and metrics as needed
        };

        // Filter by goal name if selected
        var filteredGoals = string.IsNullOrEmpty(goal)
            ? allGoals
            : allGoals.Where(g => g.Name == goal).ToList();

        // Filter metrics by status and time period
        foreach (var g in filteredGoals)
        {
            foreach (var s in g.Strategies)
            {
                s.Metrics = s.Metrics
                    .Where(m => (string.IsNullOrEmpty(status) || m.Status == status)
                             && (string.IsNullOrEmpty(time) || m.TimePeriod == time))
                    .ToList();
            }
        }


        return View(new DashboardViewModel { StrategicGoals = filteredGoals });
    }
}