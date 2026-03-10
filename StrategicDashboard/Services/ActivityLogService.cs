using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
//Talijah's
namespace OneJaxDashboard.Services
{
    public class ActivityLogService
    {
        private readonly ApplicationDbContext _db;

        public ActivityLogService(ApplicationDbContext db)
        {
            _db = db;
        }

        public IEnumerable<ActivityLogEntry> GetRecent(string username, int take = 10)
            => _db.ActivityLogs
                .AsNoTracking()
                .Where(e => string.Equals(e.User, username, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(e => e.Timestamp)
                .Take(take)
                .ToList();

        public IEnumerable<ActivityLogEntry> GetAllEntries()
            => _db.ActivityLogs
                .AsNoTracking()
                .OrderByDescending(e => e.Timestamp)
                .ToList();

        public IEnumerable<ActivityLogEntry> GetRecentForUser(IEnumerable<string> identifiers, int take = 10)
        {
            var matches = BuildIdentifierSet(identifiers);
            return _db.ActivityLogs
                .AsNoTracking()
                .Where(e => matches.Contains(e.User))
                .OrderByDescending(e => e.Timestamp)
                .Take(take)
                .ToList();
        }

        public IEnumerable<ActivityLogEntry> GetAllForUser(IEnumerable<string> identifiers)
        {
            var matches = BuildIdentifierSet(identifiers);
            return _db.ActivityLogs
                .AsNoTracking()
                .Where(e => matches.Contains(e.User))
                .OrderByDescending(e => e.Timestamp)
                .ToList();
        }

        public void Log(string user, string action, string? entity = null, string? details = null)
        {
            _db.ActivityLogs.Add(new ActivityLogEntry
            {
                Timestamp = DateTime.UtcNow,
                User = user,
                Action = action,
                Entity = entity,
                Details = details
            });
            _db.SaveChanges();
        }

        private static HashSet<string> BuildIdentifierSet(IEnumerable<string> identifiers)
        {
            return identifiers
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .Select(i => i.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
    }
}
