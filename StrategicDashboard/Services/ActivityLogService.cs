using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Data;
using OneJaxDashboard.Models;
using Microsoft.Extensions.Logging;
//Talijah's
namespace OneJaxDashboard.Services
{
    public class ActivityLogService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ActivityLogService> _logger;
        private bool _schemaRepairAttempted;

        public ActivityLogService(ApplicationDbContext db, ILogger<ActivityLogService> logger)
        {
            _db = db;
            _logger = logger;
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
            var entry = new ActivityLogEntry
            {
                Timestamp = DateTime.UtcNow,
                User = user,
                Action = action,
                Entity = entity,
                Details = details
            };

            try
            {
                _db.ActivityLogs.Add(entry);
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Activity log write failed for user '{User}' and action '{Action}'.",
                    user,
                    action);

                if (_schemaRepairAttempted)
                {
                    return;
                }

                _schemaRepairAttempted = true;
                if (!TryRepairActivityLogSchema())
                {
                    return;
                }

                try
                {
                    _db.ActivityLogs.Add(entry);
                    _db.SaveChanges();
                }
                catch (Exception retryEx)
                {
                    _logger.LogWarning(
                        retryEx,
                        "Activity log retry failed for user '{User}' and action '{Action}'.",
                        user,
                        action);
                }
            }
        }

        private bool TryRepairActivityLogSchema()
        {
            try
            {
                if (_db.Database.IsSqlServer())
                {
                    _db.Database.ExecuteSqlRaw("""
                        IF OBJECT_ID(N'dbo.ActivityLogs', N'U') IS NULL
                        BEGIN
                            CREATE TABLE [dbo].[ActivityLogs]
                            (
                                [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_ActivityLogs] PRIMARY KEY,
                                [Timestamp] DATETIME2 NOT NULL CONSTRAINT [DF_ActivityLogs_Timestamp] DEFAULT(SYSUTCDATETIME()),
                                [User] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_ActivityLogs_User] DEFAULT(N''),
                                [Action] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_ActivityLogs_Action] DEFAULT(N''),
                                [Entity] NVARCHAR(MAX) NULL,
                                [Details] NVARCHAR(MAX) NULL
                            );
                        END

                        IF COL_LENGTH('dbo.ActivityLogs', 'Entity') IS NULL
                        BEGIN
                            ALTER TABLE [dbo].[ActivityLogs] ADD [Entity] NVARCHAR(MAX) NULL;
                        END

                        IF COL_LENGTH('dbo.ActivityLogs', 'Details') IS NULL
                        BEGIN
                            ALTER TABLE [dbo].[ActivityLogs] ADD [Details] NVARCHAR(MAX) NULL;
                        END
                        """);

                    return true;
                }

                if (_db.Database.IsSqlite())
                {
                    _db.Database.ExecuteSqlRaw("""
                        CREATE TABLE IF NOT EXISTS "ActivityLogs" (
                            "Id" INTEGER NOT NULL CONSTRAINT "PK_ActivityLogs" PRIMARY KEY AUTOINCREMENT,
                            "Timestamp" TEXT NOT NULL,
                            "User" TEXT NOT NULL,
                            "Action" TEXT NOT NULL,
                            "Entity" TEXT NULL,
                            "Details" TEXT NULL
                        );
                        """);

                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Activity log schema repair failed.");
            }

            return false;
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
