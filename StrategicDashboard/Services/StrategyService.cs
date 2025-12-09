using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;

//Dina's , ask for in case 
namespace OneJaxDashboard.Services
{
    public class StrategyService
    {
        private readonly ApplicationDbContext _context;

        public StrategyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StrategicGoal>> GetAllStrategicGoalsAsync()
        {
            return await _context.StrategicGoals
                .Include(g => g.Metrics)
                .Include(g => g.Strategies)
                .ToListAsync();
        }

        public IEnumerable<StrategicGoal> GetAllStrategicGoals()
        {
            return _context.StrategicGoals
                .Include(g => g.Metrics)
                .Include(g => g.Strategies)
                .ToList();
        }

        public StrategicGoal? GetStrategicGoal(int id)
        {
            return _context.StrategicGoals
                .Include(g => g.Metrics)
                .Include(g => g.Strategies)
                .FirstOrDefault(g => g.Id == id);
        }

        public IEnumerable<Strategy> GetAllStrategies()
        {
            return _context.Strategies
                .Include(s => s.StrategicGoal)
                .ToList();
        }

        public IEnumerable<Strategy> GetStrategiesByGoal(int goalId)
        {
            return _context.Strategies
                .Where(s => s.StrategicGoalId == goalId)
                .Include(s => s.StrategicGoal)
                .ToList();
        }

        public Strategy? GetStrategy(int id)
        {
            return _context.Strategies
                .Include(s => s.StrategicGoal)
                .FirstOrDefault(s => s.Id == id);
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