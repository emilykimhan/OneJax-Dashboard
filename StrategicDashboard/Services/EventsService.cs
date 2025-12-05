using OneJaxDashboard.Models;

namespace OneJaxDashboard.Services
{
    public class EventsService
    {
        private static readonly List<Event> _events = new();

        public IEnumerable<Event> GetByOwner(string username)
            => _events.Where(e => string.Equals(e.OwnerUsername, username, StringComparison.OrdinalIgnoreCase));

        public Event? Get(int id) => _events.FirstOrDefault(e => e.Id == id);

        public Event Add(Event eventModel)
        {
            eventModel.Id = _events.Count > 0 ? _events.Max(e => e.Id) + 1 : 1;
            _events.Add(eventModel);
            return eventModel;
        }

        public void Update(Event eventModel)
        {
            var existing = Get(eventModel.Id);
            if (existing == null) return;
            
            existing.Title = eventModel.Title;
            existing.Description = eventModel.Description;
            existing.Status = eventModel.Status;
            existing.StartDate = eventModel.StartDate;
            existing.EndDate = eventModel.EndDate;
            existing.StrategicGoalId = eventModel.StrategicGoalId;
            existing.StrategyId = eventModel.StrategyId;
        }

        public void Remove(int id) => _events.RemoveAll(e => e.Id == id);

        public IEnumerable<Event> GetAll() => _events;

        public IEnumerable<Event> GetByStrategy(int strategyId)
            => _events.Where(e => e.StrategyId == strategyId);

        public IEnumerable<Event> GetByStrategicGoal(int goalId)
            => _events.Where(e => e.StrategicGoalId == goalId);
    }
}