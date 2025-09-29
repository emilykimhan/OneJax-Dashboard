using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
        public IActionResult Index()
        {
            var model = new DashboardViewModel
            {
                UserRole = "Viewer",
                StrategicGoals = new List<StrategicGoalSummary>
            {
                new StrategicGoalSummary { GoalName = "Increase Engagement", ProgressPercent = 75, StatusColor = "Green" },
                new StrategicGoalSummary { GoalName = "Expand Programs", ProgressPercent = 40, StatusColor = "Red" }
            },
                RecentProgramsEvents = new List<ProgramEventSummary>
            {
                new ProgramEventSummary { Name = "Community Forum", Date = DateTime.Now.AddDays(-2), Type = "Event" }
            },
                Metrics = new List<MetricSummary>
            {
                new MetricSummary { MetricName = "Attendance", Value = 120, Unit = "People", Date = DateTime.Now, GoalAlignment = "Increase Engagement", StatusColor = "Green" }
            },
                Alerts = new List<AlertSummary>
            {
                new AlertSummary { Message = "Submit quarterly report", DueDate = DateTime.Now.AddDays(5), IsCritical = true }
            },
                SelectedTimeFilter = "Monthly",
                SelectedGoal = "Increase Engagement"
            };

            return View(model);
        }
}