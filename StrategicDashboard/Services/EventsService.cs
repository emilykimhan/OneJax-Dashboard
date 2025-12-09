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

        public IEnumerable<Event> GetAll()
        {
            return _db.Events.Include(e => e.StrategicGoal).Include(e => e.Strategy).Where(e => !e.IsArchived).ToList();
        }

        public IEnumerable<Event> GetArchived()
        {
            return _db.Events.Include(e => e.StrategicGoal).Include(e => e.Strategy).Where(e => e.IsArchived).ToList();
        }

        public IEnumerable<Event> GetByOwner(string username)
        {
            return _db.Events.Include(e => e.StrategicGoal).Include(e => e.Strategy).Where(e => e.OwnerUsername == username && !e.IsArchived).ToList();
        }

        public IEnumerable<Event> GetArchivedByOwner(string username)
        {
            return _db.Events.Include(e => e.StrategicGoal).Include(e => e.Strategy).Where(e => e.OwnerUsername == username && e.IsArchived).ToList();
        }

        public Event? Get(int id)
        {
            return _db.Events.Include(e => e.StrategicGoal).Include(e => e.Strategy).FirstOrDefault(e => e.Id == id);
        }

        public void Add(Event eventItem)
        {
            _db.Events.Add(eventItem);
            _db.SaveChanges();
        }

        public void Update(Event eventItem)
        {
            var existing = _db.Events.FirstOrDefault(e => e.Id == eventItem.Id);
            if (existing != null)
            {
                existing.Title = eventItem.Title;
                existing.Description = eventItem.Description;
                existing.StartDate = eventItem.StartDate;
                existing.EndDate = eventItem.EndDate;
                existing.Location = eventItem.Location;
                existing.Attendees = eventItem.Attendees;
                existing.Status = eventItem.Status;
                existing.SatisfactionScore = eventItem.SatisfactionScore;
                existing.Notes = eventItem.Notes;
                existing.PreAssessmentData = eventItem.PreAssessmentData;
                existing.PostAssessmentData = eventItem.PostAssessmentData;
                existing.OwnerUsername = eventItem.OwnerUsername;
                existing.IsAssignedByAdmin = eventItem.IsAssignedByAdmin;
                existing.AssignmentDate = eventItem.AssignmentDate;
                existing.CompletionDate = eventItem.CompletionDate;
                existing.DueDate = eventItem.DueDate;
                existing.StrategicGoalId = eventItem.StrategicGoalId;
                existing.StrategyId = eventItem.StrategyId;
                existing.AdminNotes = eventItem.AdminNotes;
                
                _db.SaveChanges();
            }
        }

        public void Remove(int id)
        {
            var eventItem = _db.Events.FirstOrDefault(e => e.Id == id);
            if (eventItem != null)
            {
                _db.Events.Remove(eventItem);
                _db.SaveChanges();
            }
        }

        public void Archive(int id)
        {
            var eventItem = _db.Events.FirstOrDefault(e => e.Id == id);
            if (eventItem != null)
            {
                eventItem.IsArchived = true;
                _db.SaveChanges();
            }
        }

        public void Unarchive(int id)
        {
            var eventItem = _db.Events.FirstOrDefault(e => e.Id == id);
            if (eventItem != null)
            {
                eventItem.IsArchived = false;
                _db.SaveChanges();
            }
        }
    }
}
