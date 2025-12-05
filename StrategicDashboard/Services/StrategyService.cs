using OneJaxDashboard.Models;
//Dina's , ask for in case 
namespace OneJaxDashboard.Services
{
    public class StrategyService
    {
        // Sample data - in a real application, this would come from a database
        private static readonly List<StrategicGoal> _strategicGoals = new()
        {
            new StrategicGoal 
            { 
                Id = 1, 
                Name = "Community Engagement",
                Strategies = new List<Strategy>
                {
                    new Strategy { Id = 1, Name = "Neighborhood Outreach", StrategicGoalId = 1 },
                    new Strategy { Id = 2, Name = "Social Media Campaign", StrategicGoalId = 1 },
                    new Strategy { Id = 3, Name = "Community Events", StrategicGoalId = 1 }
                }
            },
            new StrategicGoal 
            { 
                Id = 2, 
                Name = "Economic Development",
                Strategies = new List<Strategy>
                {
                    new Strategy { Id = 4, Name = "Business Partnership Program", StrategicGoalId = 2 },
                    new Strategy { Id = 5, Name = "Workforce Training", StrategicGoalId = 2 },
                    new Strategy { Id = 6, Name = "Small Business Support", StrategicGoalId = 2 }
                }
            },
            new StrategicGoal 
            { 
                Id = 3, 
                Name = "Infrastructure & Technology",
                Strategies = new List<Strategy>
                {
                    new Strategy { Id = 7, Name = "Digital Infrastructure", StrategicGoalId = 3 },
                    new Strategy { Id = 8, Name = "Transportation Improvements", StrategicGoalId = 3 },
                    new Strategy { Id = 9, Name = "Technology Upgrades", StrategicGoalId = 3 }
                }
            },
            new StrategicGoal 
            { 
                Id = 4, 
                Name = "Health & Wellness",
                Strategies = new List<Strategy>
                {
                    new Strategy { Id = 10, Name = "Health Education Programs", StrategicGoalId = 4 },
                    new Strategy { Id = 11, Name = "Community Health Services", StrategicGoalId = 4 },
                    new Strategy { Id = 12, Name = "Wellness Initiatives", StrategicGoalId = 4 }
                }
            }
        };

        private static readonly List<Strategy> _allStrategies = _strategicGoals
            .SelectMany(g => g.Strategies)
            .ToList();

        public IEnumerable<StrategicGoal> GetAllStrategicGoals()
        {
            return _strategicGoals;
        }

        public StrategicGoal? GetStrategicGoal(int id)
        {
            return _strategicGoals.FirstOrDefault(g => g.Id == id);
        }

        public IEnumerable<Strategy> GetAllStrategies()
        {
            return _allStrategies;
        }

        public IEnumerable<Strategy> GetStrategiesByGoal(int goalId)
        {
            return _allStrategies.Where(s => s.StrategicGoalId == goalId);
        }

        public Strategy? GetStrategy(int id)
        {
            return _allStrategies.FirstOrDefault(s => s.Id == id);
        }

        public string GetStrategyName(int? strategyId)
        {
            if (!strategyId.HasValue) return "No Strategy Assigned";
            var strategy = GetStrategy(strategyId.Value);
            return strategy?.Name ?? "Unknown Strategy";
        }

        public string GetStrategicGoalName(int? goalId)
        {
            if (!goalId.HasValue) return "No Goal Assigned";
            var goal = GetStrategicGoal(goalId.Value);
            return goal?.Name ?? "Unknown Goal";
        }
    }
}