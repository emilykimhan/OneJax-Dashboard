using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;

namespace OneJaxDashboard.Services
{
    public class StrategyService
    {
        private readonly ApplicationDbContext _db;

        public StrategyService(ApplicationDbContext db)
        {
            _db = db;
        }

        // Get all strategies from the Strategies table in DB
        public IEnumerable<Strategy> GetAllStrategies()
        {
            return _db.Strategies.ToList();
        }

        // Get strategies by strategic goal
        public IEnumerable<Strategy> GetStrategiesByGoal(int goalId)
        {
            return _db.Strategies.Where(s => s.StrategicGoalId == goalId).ToList();
        }

        // Get a single strategy by ID
        public Strategy? GetStrategy(int id)
        {
            return _db.Strategies.FirstOrDefault(s => s.Id == id);
        }

        // Get all strategic goals
        public IEnumerable<StrategicGoal> GetAllStrategicGoals()
        {
            var strategies = _db.Strategies.ToList();
            
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

        // Get a single strategic goal
        public StrategicGoal? GetStrategicGoal(int id)
        {
            return GetAllStrategicGoals().FirstOrDefault(g => g.Id == id);
        }

        // Get strategy name
        public string GetStrategyName(int? strategyId)
        {
            if (!strategyId.HasValue) return "No Strategy Assigned";
            var strategy = GetStrategy(strategyId.Value);
            return strategy?.Name ?? "Unknown Strategy";
        }

        // Get strategic goal name
        public string GetStrategicGoalName(int? goalId)
        {
            if (!goalId.HasValue) return "No Goal Assigned";
            return GetGoalNameById(goalId ?? 0);
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
    }
}