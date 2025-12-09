using OneJaxDashboard.Models;
//Dina's , ask for in case 
namespace OneJaxDashboard.Services
{
    public class StrategyService
    {
        // Clean slate - data will come from database
        private static readonly List<StrategicGoal> _strategicGoals = new();
        private static readonly List<Strategy> _allStrategies = new();

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