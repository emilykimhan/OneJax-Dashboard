using Microsoft.AspNetCore.Mvc;
using OneJax.StrategicDashboard.Models;
using System.Collections.Generic;
using System.Linq;

public class HomeController : Controller
{
    // FIXED 3-YEAR STRATEGIC PLAN - These goals with sample events
    private static readonly List<StrategicGoal> _threeYearPlan = new List<StrategicGoal>
    {
        new StrategicGoal
        {
            Id = 1,
            Name = "Community Engagement",
            Description = "Building partnerships and community connections",
            Color = "var(--onejax-blue)",
            Events = new List<Event>(),
            Metrics = new List<GoalMetric>
            {
                new GoalMetric
                {
                    Id = 1,
                    Name = "Joint Initiatives Partner Satisfaction",
                    Description = "Launch a minimum of 3 joint initiatives with partner satisfaction ratings of at least 85%",
                    StrategicGoalId = 1,
                    Target = "85",
                    CurrentValue = 85m, // Average satisfaction rating
                    Unit = "% avg",
                    Status = "Active",
                    TargetDate = new DateTime(2026, 6, 30)
                },
                new GoalMetric
                {
                    Id = 2,
                    Name = "Cross-Sector Collaborations Growth",
                    Description = "Increase the number of unique cross-sector collaborations from 3 to 10 by end of FY 26-27",
                    StrategicGoalId = 1,
                    Target = "10",
                    CurrentValue = 3m, // Current number of collaborations
                    Unit = "collaborations",
                    Status = "Active",
                    TargetDate = new DateTime(2027, 6, 30),
                    Q1Value = 3, // Quarterly tracking
                    Q2Value = 0,
                    Q3Value = 0,
                    Q4Value = 0
                },
                new GoalMetric
                {
                    Id = 3,
                    Name = "Interfaith Collaborative Events",
                    Description = "Host at least 4 interfaith collaborative events during FY 25-26",
                    StrategicGoalId = 1,
                    Target = "4",
                    CurrentValue = 5m, // Current events hosted
                    Unit = "events",
                    Status = "Active",
                    TargetDate = new DateTime(2026, 6, 30)
                },
                new GoalMetric
                {
                    Id = 4,
                    Name = "Event Attendee Satisfaction",
                    Description = "Event attendee satisfaction over 85%",
                    StrategicGoalId = 1,
                    Target = "85",
                    CurrentValue = 85m, // Average satisfaction rating
                    Unit = "% avg",
                    Status = "Active",
                    TargetDate = new DateTime(2026, 6, 30)
                },
                new GoalMetric
                {
                    Id = 5,
                    Name = "Faith Representation at Events",
                    Description = "Ensure 3 faiths represented at 80% of community events",
                    StrategicGoalId = 1,
                    Target = "3",
                    CurrentValue = 3m, // Average faiths represented
                    Unit = "faiths avg",
                    Status = "Active",
                    TargetDate = new DateTime(2026, 6, 30)
                },
                new GoalMetric
                {
                    Id = 6,
                    Name = "Clergy & Interfaith Network Expansion",
                    Description = "Expand clergy and interfaith network contacts by 25% by end of FY 25-26",
                    StrategicGoalId = 1,
                    Target = "25",
                    CurrentValue = 15m, // Current growth percentage
                    Unit = "%",
                    Status = "Active",
                    TargetDate = new DateTime(2026, 6, 30),
                    Q1Value = 255, // Total contacts - quarterly tracking
                    Q2Value = 0,
                    Q3Value = 0,
                    Q4Value = 0
                },
                new GoalMetric
                {
                    Id = 7,
                    Name = "Youth Attendance Growth",
                    Description = "Increase youth attendance by at least 20% across programs",
                    StrategicGoalId = 1,
                    Target = "20",
                    CurrentValue = 18m, // Current growth percentage
                    Unit = "%",
                    Status = "Active",
                    TargetDate = new DateTime(2026, 6, 30),
                    Q1Value = 226, // Total attendees: Metrotown(16) + MIAD(100) + Connect!(100) + LOUD(10)
                    Q2Value = 0,
                    Q3Value = 0,
                    Q4Value = 0
                },
                new GoalMetric
                {
                    Id = 8,
                    Name = "Youth Program Satisfaction",
                    Description = "Secure an average participant satisfaction rating of 85% or higher through post-event surveys",
                    StrategicGoalId = 1,
                    Target = "85",
                    CurrentValue = 85m, // Average satisfaction rating
                    Unit = "% avg",
                    Status = "Active",
                    TargetDate = new DateTime(2026, 6, 30)
                },
                new GoalMetric
                {
                    Id = 9,
                    Name = "Skills Assessment Improvement",
                    Description = "Demonstrate at least a 10% improvement in pre- and post-program assessments of resilience and communication skills",
                    StrategicGoalId = 1,
                    Target = "10",
                    CurrentValue = 90m, // Current improvement percentage (far exceeding target!)
                    Unit = "%",
                    Status = "Active",
                    TargetDate = new DateTime(2026, 6, 30)
                }
            }
        },
        new StrategicGoal
        {
            Id = 2,
            Name = "Identity/Value Proposition",
            Description = "Establishing and communicating OneJax's unique identity and value",
            Color = "var(--onejax-orange)",
            Events = new List<Event>(),
            Metrics = new List<GoalMetric>
            {
                new GoalMetric
                {
                    Id = 1,
                    Name = "Earned Media Placements",
                    Description = "Achieve a minimum of 12 earned media placements by December 2026",
                    StrategicGoalId = 2,
                    Target = "12",
                    CurrentValue = 3,
                    Unit = "placements",
                    Status = "Active",
                    TargetDate = new DateTime(2026, 12, 31)
                },
                new GoalMetric
                {
                    Id = 2,
                    Name = "Website Traffic Growth",
                    Description = "Increase overall website traffic by 25%",
                    StrategicGoalId = 2,
                    Target = "25",
                    CurrentValue = 8.5m, // Current growth percentage based on quarterly data
                    Unit = "%",
                    Status = "Active",
                    TargetDate = new DateTime(2025, 12, 31),
                    Q1Value = 12450, // Website visitors in Q1
                    Q2Value = 13200, // Website visitors in Q2 
                    Q3Value = 14100, // Website visitors in Q3
                    Q4Value = 0 // Q4 data not available yet (future quarter)
                },
                new GoalMetric
                {
                    Id = 3,
                    Name = "Social Media Engagement Growth",
                    Description = "Boost social media engagement by 30% within 12 months",
                    StrategicGoalId = 2,
                    Target = "30",
                    CurrentValue = 12.5m, // Current engagement growth percentage
                    Unit = "%",
                    Status = "Active",
                    TargetDate = new DateTime(2025, 10, 31),
                    Q1Value = 0, // Engagement metrics Q1 - to be filled
                    Q2Value = 0, // Engagement metrics Q2 - to be filled
                    Q3Value = 0, // Engagement metrics Q3 - to be filled
                    Q4Value = 0 // Engagement metrics Q4 - to be filled
                },
                new GoalMetric
                {
                    Id = 4,
                    Name = "Key Plan Milestones Achievement",
                    Description = "Achieve 75% of key plan milestones (content calendar, press releases, brand messaging) by the 6-month review",
                    StrategicGoalId = 2,
                    Target = "75",
                    CurrentValue = 45m, // Current milestone completion percentage
                    Unit = "%",
                    Status = "Active",
                    TargetDate = new DateTime(2025, 4, 30)
                },
                new GoalMetric
                {
                    Id = 5,
                    Name = "Community Perception Survey Results",
                    Description = "Conduct a community perception survey biannually, aiming for at least 70% of respondents to identify OneJax as a trusted leader by Q4 2025",
                    StrategicGoalId = 2,
                    Target = "70",
                    CurrentValue = 70m, // Current average perception score
                    Unit = "% avg",
                    Status = "Active",
                    TargetDate = new DateTime(2025, 12, 31)
                },
                new GoalMetric
                {
                    Id = 6,
                    Name = "Participant Demographics Expansion",
                    Description = "Expand program and event participant demographics to reflect at least a 20% increase in representation from underrepresented ZIP codes or demographic groups by end of 2025",
                    StrategicGoalId = 2,
                    Target = "20",
                    CurrentValue = 8m, // Current increase in representation
                    Unit = "%",
                    Status = "Active",
                    TargetDate = new DateTime(2025, 12, 31)
                }
            }
        },
        new StrategicGoal
        {
            Id = 3,
            Name = "Financial Stability",
            Description = "Ensuring sustainable financial health and growth",
            Color = "var(--onejax-green)",
            Events = new List<Event>(),
            Metrics = new List<GoalMetric>()
        },
        new StrategicGoal
        {
            Id = 4,
            Name = "Organizational Building",
            Description = "Strengthening organizational structure and capacity",
            Color = "var(--onejax-navy)",
            Events = new List<Event>(),
            Metrics = new List<GoalMetric>()
        }
    };

    public IActionResult Index(string status, string time, string goal)
    {
        // Work with the FIXED 3-year plan (create copy to avoid modifying original)
        var allGoals = _threeYearPlan.Select(g => new StrategicGoal
        {
            Id = g.Id,
            Name = g.Name,
            Description = g.Description,
            Color = g.Color,
            Events = g.Events.Select(e => new Event
            {
                Id = e.Id,
                Title = e.Title,
                Date = e.Date,
                Type = e.Type,
                Location = e.Location,
                StrategicGoalId = e.StrategicGoalId,
                Status = e.Status,
                PreAssessmentData = e.PreAssessmentData,
                PostAssessmentData = e.PostAssessmentData,
                Attendees = e.Attendees,
                SatisfactionScore = e.SatisfactionScore,
                Notes = e.Notes
            }).ToList(),
            Metrics = g.Metrics.Select(m => new GoalMetric
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                StrategicGoalId = m.StrategicGoalId,
                Target = m.Target,
                CurrentValue = m.CurrentValue,
                Unit = m.Unit,
                Status = m.Status,
                TargetDate = m.TargetDate,
                Q1Value = m.Q1Value,
                Q2Value = m.Q2Value,
                Q3Value = m.Q3Value,
                Q4Value = m.Q4Value
            }).ToList()
        }).ToList();

        // Don't filter goals - always show all goals, just filter events within them
        var filteredGoals = allGoals;

        // Filter events by status and time period
        foreach (var g in filteredGoals)
        {
            g.Events = g.Events
                .Where(e => (string.IsNullOrEmpty(status) || e.Status == status))
                .ToList();
                
            // Time filtering can be added here later if needed
            // && (string.IsNullOrEmpty(time) || [time period logic])
        }

        return View(new DashboardViewModel { StrategicGoals = filteredGoals });
    }

    // Static methods for other controllers to access the fixed plan
    public static List<StrategicGoal> GetThreeYearPlan()
    {
        return _threeYearPlan;
    }

    public static Event? GetEvent(int eventId)
    {
        return _threeYearPlan.SelectMany(g => g.Events)
                           .FirstOrDefault(e => e.Id == eventId);
    }

    public static void AddEventToGoal(int goalId, Event eventItem)
    {
        var goal = _threeYearPlan.FirstOrDefault(g => g.Id == goalId);
        if (goal != null)
        {
            eventItem.Id = goal.Events.Any() ? goal.Events.Max(e => e.Id) + 1 : 1;
            eventItem.StrategicGoalId = goalId;
            goal.Events.Add(eventItem);
        }
    }

    public static List<Event> GetEventsByGoal(int goalId)
    {
        var goal = _threeYearPlan.FirstOrDefault(g => g.Id == goalId);
        return goal?.Events ?? new List<Event>();
    }
}