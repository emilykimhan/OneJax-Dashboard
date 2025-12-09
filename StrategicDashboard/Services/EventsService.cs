using OneJaxDashboard.Models;

namespace OneJaxDashboard.Services
{
    public class EventsService
    {
        private static readonly List<Event> _events = new();

        public IEnumerable<Event> GetByOwner(string username)
            => _events.Where(e => string.Equals(e.OwnerUsername, username, StringComparison.OrdinalIgnoreCase) && !e.IsArchived);

        public IEnumerable<Event> GetArchivedByOwner(string username)
            => _events.Where(e => string.Equals(e.OwnerUsername, username, StringComparison.OrdinalIgnoreCase) && e.IsArchived);

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
            
            existing.StrategyTemplateId = eventModel.StrategyTemplateId;
            existing.Title = eventModel.Title;
            existing.Description = eventModel.Description;
            existing.Status = eventModel.Status;
            existing.StartDate = eventModel.StartDate;
            existing.EndDate = eventModel.EndDate;
            existing.StrategicGoalId = eventModel.StrategicGoalId;
            existing.StrategyId = eventModel.StrategyId;
            existing.DueDate = eventModel.DueDate;
            existing.AdminNotes = eventModel.AdminNotes;
            existing.Type = eventModel.Type;
            existing.Location = eventModel.Location;
            existing.SatisfactionScore = eventModel.SatisfactionScore;
            existing.Attendees = eventModel.Attendees;
            existing.Notes = eventModel.Notes;
            existing.PreAssessmentData = eventModel.PreAssessmentData;
            existing.PostAssessmentData = eventModel.PostAssessmentData;
            existing.IsArchived = eventModel.IsArchived;
            existing.CompletionDate = eventModel.CompletionDate;
        }

        public void Archive(int id)
        {
            var existing = Get(id);
            if (existing == null) return;
            existing.IsArchived = true;
            existing.CompletionDate = DateTime.Now;
        }

        public void Unarchive(int id)
        {
            var existing = Get(id);
            if (existing == null) return;
            existing.IsArchived = false;
        }

        public void Remove(int id) => _events.RemoveAll(e => e.Id == id);

        public IEnumerable<Event> GetAll() => _events.Where(e => !e.IsArchived);

        public IEnumerable<Event> GetAllIncludingArchived() => _events;

        public IEnumerable<Event> GetArchived() => _events.Where(e => e.IsArchived);

        public IEnumerable<Event> GetByStrategy(int strategyId)
            => _events.Where(e => e.StrategyId == strategyId && !e.IsArchived);

        public IEnumerable<Event> GetByStrategicGoal(int goalId)
            => _events.Where(e => e.StrategicGoalId == goalId && !e.IsArchived);

        public IEnumerable<Event> GetByStrategyTemplate(int strategyTemplateId)
            => _events.Where(e => e.StrategyTemplateId == strategyTemplateId && !e.IsArchived);

        // Remove events that reference deleted strategies
        public void RemoveByStrategyTemplate(int strategyTemplateId)
            => _events.RemoveAll(e => e.StrategyTemplateId == strategyTemplateId);
    }
}