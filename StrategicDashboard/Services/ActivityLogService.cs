using OneJaxDashboard.Models;
using OneJaxDashboard.Data;
using Microsoft.EntityFrameworkCore;
//Talijah's
namespace OneJaxDashboard.Services
{
    public class ActivityLogService
    {
        private readonly ApplicationDbContext _context;

        public ActivityLogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<ActivityLogEntry> GetRecent(string username, int take = 10)
            => _context.ActivityLogs
                       .Where(e => string.Equals(e.Username, username, StringComparison.OrdinalIgnoreCase))
                       .OrderByDescending(e => e.Timestamp)
                       .Take(take)
                       .ToList();

        public IEnumerable<ActivityLogEntry> GetAllEntries()
            => _context.ActivityLogs
                       .OrderByDescending(e => e.Timestamp)
                       .ToList();

        public IEnumerable<ActivityLogEntry> GetEntriesByEntityId(string entityType, int entityId)
            => _context.ActivityLogs
                       .Where(e => e.EntityType == entityType && e.EntityId == entityId)
                       .OrderByDescending(e => e.Timestamp)
                       .ToList();

        public void Log(string username, string action, string? entityType = null, int? entityId = null, string? notes = null)
        {
            var entry = new ActivityLogEntry
            {
                Username = username,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Timestamp = DateTime.UtcNow,
                Notes = notes
            };

            _context.ActivityLogs.Add(entry);
            _context.SaveChanges();
        }
    }
}
