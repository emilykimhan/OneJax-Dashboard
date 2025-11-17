using StrategicDashboard.Models;

namespace StrategicDashboard.Services
{
    public class ActivityLogService
    {
        private static readonly List<ActivityLogEntry> _entries = new();

        public IEnumerable<ActivityLogEntry> GetRecent(string username, int take = 10)
            => _entries.Where(e => string.Equals(e.Username, username, StringComparison.OrdinalIgnoreCase))
                       .OrderByDescending(e => e.Timestamp)
                       .Take(take);

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
    }
}
