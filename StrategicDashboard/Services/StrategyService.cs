using OneJaxDashboard.Models;
//Dina's , ask for in case 
namespace OneJaxDashboard.Services
{
    public class StrategyService
    {
        // Reference the actual strategies from StrategyController
        private List<Strategy> GetStrategiesFromController()
        {
            return StrategyController.Strategies;
        }

        // Get all strategic goals from the strategies
        private List<StrategicGoal> GetStrategicGoalsFromStrategies()
        {
            var strategies = GetStrategiesFromController();
            
            // Group strategies by their StrategicGoalId
            var goalGroups = strategies.GroupBy(s => s.StrategicGoalId);
            
            var goals = new List<StrategicGoal>();
            foreach (var group in goalGroups)
            {
                var goalId = group.Key;
                var goalName = GetGoalNameById(goalId);
                
                goals.Add(new StrategicGoal
                {
                    Id = goalId,
                    Name = goalName,
                    Strategies = group.ToList()
                });
            }
            
            return goals;
        }

        private string GetGoalNameById(int goalId)
        {
            return goalId switch
            {
                1 => "Organizational Building",
                2 => "Financial Sustainability",
                3 => "Identity/Value Proposition",
                4 => "Community Engagement",
                _ => "Unknown Goal"
            };
        }

        public IEnumerable<StrategicGoal> GetAllStrategicGoals()
        {
            return GetStrategicGoalsFromStrategies();
        }

        public StrategicGoal? GetStrategicGoal(int id)
        {
            return GetStrategicGoalsFromStrategies().FirstOrDefault(g => g.Id == id);
        }

        public IEnumerable<Strategy> GetAllStrategies()
        {
            return GetStrategiesFromController();
        }

        public IEnumerable<Strategy> GetStrategiesByGoal(int goalId)
        {
            return GetStrategiesFromController().Where(s => s.StrategicGoalId == goalId);
        }

        public Strategy? GetStrategy(int id)
        {
            return GetStrategiesFromController().FirstOrDefault(s => s.Id == id);
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