using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;

namespace OneJaxDashboard.Services
{
    public class EventsService
    {
        private readonly ApplicationDbContext _db;
        
        public EventsService(ApplicationDbContext db)
        {
            _db = db;
        }

        public IEnumerable<Event> GetByOwner(string username)
            => _db.Events
                .Include(e => e.AssignedStaff)
                .Where(e => string.Equals(e.OwnerUsername, username, StringComparison.OrdinalIgnoreCase) && !e.IsArchived)
                .ToList();

        public IEnumerable<Event> GetArchivedByOwner(string username)
            => _db.Events
                .Include(e => e.AssignedStaff)
                .Where(e => string.Equals(e.OwnerUsername, username, StringComparison.OrdinalIgnoreCase) && e.IsArchived)
                .ToList();

        public Event? Get(int id) => _db.Events
                .Include(e => e.AssignedStaff)
                .FirstOrDefault(e => e.Id == id);

        public Event Add(Event eventModel)
        {
            _db.Events.Add(eventModel);
            _db.SaveChanges();
            return eventModel;
        }

        public void Update(Event eventModel)
        {
            var existing = _db.Events.Find(eventModel.Id);
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
            _db.SaveChanges();
        }

        public void Archive(int id)
        {
            var existing = _db.Events.Find(id);
            if (existing == null) return;
            existing.IsArchived = true;
            existing.CompletionDate = DateTime.Now;
            _db.SaveChanges();
        }

        public void Unarchive(int id)
        {
            var existing = _db.Events.Find(id);
            if (existing == null) return;
            existing.IsArchived = false;
            _db.SaveChanges();
        }

        public void Remove(int id)
        {
            var eventModel = _db.Events.Find(id);
            if (eventModel != null)
            {
            _db.Events.Remove(eventModel);
            _db.SaveChanges();
            }
        }
        public IEnumerable<Event> GetAll() 
            => _db.Events
                .Include(e => e.AssignedStaff)
                .Where(e => !e.IsArchived)
                .ToList();

        public IEnumerable<Event> GetAllIncludingArchived() => _db.Events
                .Include(e => e.AssignedStaff)
                .ToList();

        public IEnumerable<Event> GetArchived() => _db.Events
                .Include(e => e.AssignedStaff)
                .Where(e => e.IsArchived)
                .ToList();

        public IEnumerable<Event> GetByStrategy(int strategyId)
            => _db.Events
                .Include(e => e.AssignedStaff)
                .Where(e => e.StrategyId == strategyId && !e.IsArchived)
                .ToList();

        public IEnumerable<Event> GetByStrategicGoal(int goalId)
            => _db.Events
                .Include(e => e.AssignedStaff)
                .Where(e => e.StrategicGoalId == goalId && !e.IsArchived)
                .ToList();

        public IEnumerable<Event> GetByStrategyTemplate(int strategyTemplateId)
            => _db.Events
                .Include(e => e.AssignedStaff)
                .Where(e => e.StrategyTemplateId == strategyTemplateId && !e.IsArchived)
                .ToList();

        // Remove events that reference deleted strategies
        public void RemoveByStrategyTemplate(int strategyTemplateId)
        {
            var eventsToRemove = _db.Events
                .Where(e => e.StrategyTemplateId == strategyTemplateId)
                .ToList();

            _db.Events.RemoveRange(eventsToRemove);
            _db.SaveChanges();
        }
    }
}