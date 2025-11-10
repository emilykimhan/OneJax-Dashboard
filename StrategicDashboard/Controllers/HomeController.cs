using Microsoft.AspNetCore.Mvc;
using OneJax.StrategicDashboard.Models;
using StrategicDashboard.Models;
using System.Collections.Generic;
using System.Linq;

public class HomeController : Controller
{
    // FIXED 3-YEAR STRATEGIC PLAN - These strategies are read-only
    private static readonly List<StrategicGoal> _threeYearPlan = new List<StrategicGoal>
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
                    StrategicGoalId = 1,
                    Metrics = new List<Metric>
                    {
                        new Metric { Id = 1, Description = "Launch joint initiatives", Target = "3 initiatives, 85% satisfaction", Progress = "2 initiatives, 80% satisfaction", Status = "Active", TimePeriod = "Monthly" },
                        new Metric { Id = 2, Description = "Increase collaborations", Target = "3 to 10 by FY 26-27", Progress = "5 collaborations", Status = "Upcoming", TimePeriod = "Yearly" }
                    }
                },
                new Strategy
                {
                    Id = 2,
                    Name = "Develop community outreach programs",
                    StrategicGoalId = 1,
                    Metrics = new List<Metric>()
                },
                new Strategy
                {
                    Id = 3,
                    Name = "Engage with local organizations",
                    StrategicGoalId = 1,
                    Metrics = new List<Metric>()
                }
            }
        },
        new StrategicGoal
        {
            Id = 2,
            Name = "Education & Awareness",
            Strategies = new List<Strategy>
            {
                new Strategy
                {
                    Id = 4,
                    Name = "Create educational content",
                    StrategicGoalId = 2,
                    Metrics = new List<Metric>()
                },
                new Strategy
                {
                    Id = 5,
                    Name = "Implement awareness campaigns",
                    StrategicGoalId = 2,
                    Metrics = new List<Metric>()
                },
                new Strategy
                {
                    Id = 6,
                    Name = "Conduct workshops and training",
                    StrategicGoalId = 2,
                    Metrics = new List<Metric>()
                }
            }
        },
        new StrategicGoal
        {
            Id = 3,
            Name = "Leadership Development",
            Strategies = new List<Strategy>
            {
                new Strategy
                {
                    Id = 7,
                    Name = "Mentor training programs",
                    StrategicGoalId = 3,
                    Metrics = new List<Metric>()
                },
                new Strategy
                {
                    Id = 8,
                    Name = "Leadership skill development",
                    StrategicGoalId = 3,
                    Metrics = new List<Metric>()
                }
            }
        },
        new StrategicGoal
        {
            Id = 4,
            Name = "Advocacy & Policy",
            Strategies = new List<Strategy>
            {
                new Strategy
                {
                    Id = 9,
                    Name = "Policy research and development",
                    StrategicGoalId = 4,
                    Metrics = new List<Metric>()
                },
                new Strategy
                {
                    Id = 10,
                    Name = "Advocacy campaign implementation",
                    StrategicGoalId = 4,
                    Metrics = new List<Metric>()
                }
            }
        }
    };

    public IActionResult Index(string status, string time, string goal)
    {
        // Work with the FIXED 3-year plan (create copy to avoid modifying original)
        var allGoals = _threeYearPlan.Select(g => new StrategicGoal
        {
            Id = g.Id,
            Name = g.Name,
            Strategies = g.Strategies.Select(s => new Strategy
            {
                Id = s.Id,
                Name = s.Name,
                StrategicGoalId = s.StrategicGoalId,
                Metrics = s.Metrics.ToList()
            }).ToList()
        }).ToList();

        // Filter by goal name if selected
        var filteredGoals = string.IsNullOrEmpty(goal)
            ? allGoals
            : allGoals.Where(g => g.Name == goal).ToList();

        // Filter metrics by status and time period (only metrics are filtered, strategies remain)
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

    // Static methods for other controllers to access the fixed plan
    public static List<StrategicGoal> GetThreeYearPlan()
    {
        return _threeYearPlan;
    }

    public static Strategy GetStrategy(int strategyId)
    {
        return _threeYearPlan.SelectMany(g => g.Strategies)
                           .FirstOrDefault(s => s.Id == strategyId);
    }

    public static void AddMetricToStrategy(int strategyId, Metric metric)
    {
        var strategy = GetStrategy(strategyId);
        if (strategy != null)
        {
            metric.Id = strategy.Metrics.Any() ? strategy.Metrics.Max(m => m.Id) + 1 : 1;
            strategy.Metrics.Add(metric);
        }
    }
}