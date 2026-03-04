using OneJaxDashboard.Models;
//Talijah's
namespace OneJaxDashboard.Services
{
    public class ActivityLogService
    {
        private static readonly List<ActivityLogEntry> _entries = new();

        public IEnumerable<ActivityLogEntry> GetRecent(string username, int take = 10)
            => _entries.Where(e => string.Equals(e.Username, username, StringComparison.OrdinalIgnoreCase))
                       .OrderByDescending(e => e.Timestamp)
                       .Take(take);

        public IEnumerable<ActivityLogEntry> GetAllEntries()
            => _entries.OrderByDescending(e => e.Timestamp);

        public IEnumerable<ActivityLogEntry> GetRecentForUser(IEnumerable<string> identifiers, int take = 10)
        {
            var matches = BuildIdentifierSet(identifiers);
            return _entries
                .Where(e => matches.Contains(e.Username))
                .OrderByDescending(e => e.Timestamp)
                .Take(take);
        }

        public IEnumerable<ActivityLogEntry> GetAllForUser(IEnumerable<string> identifiers)
        {
            var matches = BuildIdentifierSet(identifiers);
            return _entries
                .Where(e => matches.Contains(e.Username))
                .OrderByDescending(e => e.Timestamp);
        }

        public IEnumerable<ActivityLogEntry> GetEntriesByEntityId(string entityType, int entityId)
            => _entries.Where(e => e.EntityType == entityType && e.EntityId == entityId)
                       .OrderByDescending(e => e.Timestamp);

        public void Log(string username, string action, string? entityType = null, int? entityId = null, string? notes = null)
        {
            _entries.Add(new ActivityLogEntry
            {
                Id = _entries.Count > 0 ? _entries.Max(e => e.Id) + 1 : 1,
                Username = username,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Timestamp = DateTime.UtcNow,
                Notes = notes
            });
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
