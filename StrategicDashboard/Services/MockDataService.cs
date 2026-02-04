using OneJaxDashboard.Models;

namespace OneJaxDashboard.Services
{
    public class MockDataService
    {
        public static List<StrategicGoal> GetDemoGoals()
        {
            return new List<StrategicGoal>
            {
                new StrategicGoal
                {
                    Id = 1,
                    Name = "Organizational Building",
                    Description = "Strengthening organizational structure and capacity",
                    Color = "var(--onejax-navy)",
                    Events = GetDemoOrganizationalEvents(),
                    Metrics = GetDemoOrganizationalMetrics()
                },
                new StrategicGoal
                {
                    Id = 2,
                    Name = "Financial Sustainability", 
                    Description = "Ensuring sustainable financial health and growth",
                    Color = "var(--onejax-green)",
                    Events = GetDemoFinancialEvents(),
                    Metrics = GetDemoFinancialMetrics()
                },
                new StrategicGoal
                {
                    Id = 3,
                    Name = "Identity/Value Proposition",
                    Description = "Establishing and communicating OneJax's unique identity",
                    Color = "var(--onejax-orange)",
                    Events = GetDemoIdentityEvents(),
                    Metrics = GetDemoIdentityMetrics()
                },
                new StrategicGoal
                {
                    Id = 4,
                    Name = "Community Engagement",
                    Description = "Building partnerships and community connections",
                    Color = "var(--onejax-blue)",
                    Events = GetDemoCommunityEvents(),
                    Metrics = GetDemoCommunityMetrics()
                }
            };
        }

        private static List<Event> GetDemoOrganizationalEvents()
        {
            return new List<Event>
            {
                new Event
                {
                    Id = 1,
                    Title = "Staff Professional Development Workshop",
                    DueDate = DateTime.Now.AddDays(15),
                    Status = "Planned",
                    StrategicGoalId = 1,
                    Type = "Training",
                    Location = "OneJax Office",
                    Attendees = 12,
                    Notes = "Quarterly skill-building session for all staff"
                },
                new Event
                {
                    Id = 2,
                    Title = "Leadership Team Retreat",
                    DueDate = DateTime.Now.AddDays(45),
                    Status = "In Planning",
                    StrategicGoalId = 1,
                    Type = "Strategic Planning",
                    Location = "TBD",
                    Attendees = 8,
                    Notes = "Annual strategic planning and goal setting"
                }
            };
        }

        private static List<GoalMetric> GetDemoOrganizationalMetrics()
        {
            return new List<GoalMetric>
            {
                new GoalMetric
                {
                    Id = 1,
                    Name = "Staff Satisfaction Rate",
                    Description = "Employee satisfaction based on quarterly surveys",
                    StrategicGoalId = 1,
                    Target = "85",
                    CurrentValue = 78,
                    Unit = "%",
                    Status = "On Track",
                    TargetDate = DateTime.Now.AddMonths(3)
                },
                new GoalMetric
                {
                    Id = 2,
                    Name = "Professional Development Hours",
                    Description = "Total training hours completed by staff",
                    StrategicGoalId = 1,
                    Target = "200",
                    CurrentValue = 145,
                    Unit = "hours",
                    Status = "On Track",
                    TargetDate = DateTime.Now.AddMonths(6)
                }
            };
        }

        private static List<Event> GetDemoFinancialEvents()
        {
            return new List<Event>
            {
                new Event
                {
                    Id = 3,
                    Title = "Annual Fundraising Gala",
                    DueDate = DateTime.Now.AddDays(60),
                    Status = "In Planning",
                    StrategicGoalId = 2,
                    Type = "Fundraising",
                    Location = "Downtown Convention Center",
                    Attendees = 250,
                    Notes = "Major annual fundraising event - $50K goal"
                },
                new Event
                {
                    Id = 4,
                    Title = "Grant Application Deadline - United Way",
                    DueDate = DateTime.Now.AddDays(30),
                    Status = "In Progress",
                    StrategicGoalId = 2,
                    Type = "Grant Application",
                    Location = "Virtual",
                    Attendees = 3,
                    Notes = "Requesting $25K for community programs"
                }
            };
        }

        private static List<GoalMetric> GetDemoFinancialMetrics()
        {
            return new List<GoalMetric>
            {
                new GoalMetric
                {
                    Id = 3,
                    Name = "Annual Revenue Target",
                    Description = "Total organizational revenue for fiscal year",
                    StrategicGoalId = 2,
                    Target = "500000",
                    CurrentValue = 325000,
                    Unit = "$",
                    Status = "On Track",
                    TargetDate = DateTime.Now.AddMonths(9)
                },
                new GoalMetric
                {
                    Id = 4,
                    Name = "Donor Retention Rate",
                    Description = "Percentage of donors who gave in previous year",
                    StrategicGoalId = 2,
                    Target = "75",
                    CurrentValue = 68,
                    Unit = "%",
                    Status = "Needs Attention",
                    TargetDate = DateTime.Now.AddMonths(6)
                }
            };
        }

        // Add similar methods for Identity and Community goals...
        private static List<Event> GetDemoIdentityEvents() => new List<Event>();
        private static List<GoalMetric> GetDemoIdentityMetrics() => new List<GoalMetric>();
        private static List<Event> GetDemoCommunityEvents() => new List<Event>();
        private static List<GoalMetric> GetDemoCommunityMetrics() => new List<GoalMetric>();
    }
}
