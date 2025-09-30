using Microsoft.AspNetCore.Mvc;
using OneJax.StrategicDashboard.Models;
using System.Collections.Generic;
using System.Linq;

public class HomeController : Controller
{
    public IActionResult Index(string status, string time, string goal)
    {
        var allGoals = StrategicGoalsHelper.All
            .Select(g => new StrategicGoal { GoalName = g, StatusColor = "#278aa2", ProgressPercent = 0 })
            .ToList();

        var allProjects = new List<Project>
        {
            new Project { Name = "Identify and cultivate partnerships", GoalName = "Community Engagement", Status = "Active", StatusColor = "#278aa2", ProgressPercent = 50, TimePeriod = "Monthly" },
            new Project { Name = "Foster Relationships among clergy", GoalName = "Community Engagement", Status = "Active", StatusColor = "#278aa2", ProgressPercent = 60, TimePeriod = "Quarterly" },
            new Project { Name = "Provide youth programs", GoalName = "Community Engagement", Status = "Upcoming", StatusColor = "#278aa2", ProgressPercent = 0, TimePeriod = "Yearly" },
            new Project { Name = "Deepen relationships with donors", GoalName = "Financial Sustainability", Status = "Completed", StatusColor = "#278aa2", ProgressPercent = 100, TimePeriod = "Yearly" },
            new Project { Name = "Integrated marketing plan", GoalName = "Identity/Value Proposition", Status = "Active", StatusColor = "#278aa2", ProgressPercent = 80, TimePeriod = "Quarterly" },
            // ...add more projects
        };

        // Start with all projects
        var filteredProjects = allProjects;

        // Filter by goal (only if a goal is selected)
        if (!string.IsNullOrEmpty(goal))
        {
            filteredProjects = filteredProjects.Where(p => p.GoalName == goal).ToList();
        }

        // Filter by status (only if a status is selected)
        if (!string.IsNullOrEmpty(status))
        {
            filteredProjects = filteredProjects.Where(p => p.Status == status).ToList();
        }

        // Filter by time (only if a time is selected)
        if (!string.IsNullOrEmpty(time))
        {
            filteredProjects = filteredProjects.Where(p => p.TimePeriod == time).ToList();
        }

        var model = new DashboardViewModel
        {
            StrategicGoals = allGoals,
            Projects = filteredProjects
        };

        return View(model);
    }
}