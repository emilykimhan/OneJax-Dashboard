using Microsoft.EntityFrameworkCore;

namespace OneJaxDashboard.Data;

public sealed class AppDataResetter
{
    private readonly ApplicationDbContext _db;
    private readonly Action<string> _log;

    private static readonly string[] TablesToClearInOrder =
    [
        "ActivityLogs",
        "ArchivedPrograms",
        "Events",
        "GoalMetrics",
        "Strategies",
        "Programs",
        "StaffSurveys_22D",
        "ProfessionalDevelopments",
        "MediaPlacements_3D",
        "WebsiteTraffic",
        "CommunicationRate",
        "Annual_average_7D",
        "demographics_8D",
        "Plan2026_24D",
        "achieveMile_6D",
        "socialMedia_5D",
        "DonorEvents_19D",
        "FeeForServices_21D",
        "income_27D",
        "BudgetTracking_28D",
        "BoardMember_29D",
        "BoardMeetingAttendance",
        "selfAssess_31D",
        "volunteerProgram_40D",
        "Interfaith_11D",
        "EventSatisfaction_12D",
        "CollabTouch_47D",
        "FaithCommunity_13D",
        "ContactsInterfaith_14D",
        "YouthAttend_15D",
        "Diversity_37D",
        "FirstTime_38D"
    ];

    public AppDataResetter(ApplicationDbContext db, Action<string>? log = null)
    {
        _db = db;
        _log = log ?? (_ => { });
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (_db.Database.IsSqlServer())
        {
            await ResetSqlServerAsync(cancellationToken);
            return;
        }

        if (_db.Database.IsSqlite())
        {
            await ResetSqliteAsync(cancellationToken);
            return;
        }

        throw new InvalidOperationException("App data reset is only supported for SQL Server and SQLite.");
    }

    private async Task ResetSqlServerAsync(CancellationToken cancellationToken)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        foreach (var tableName in TablesToClearInOrder)
        {
            _log($"Clearing {tableName}...");
            await _db.Database.ExecuteSqlRawAsync($"DELETE FROM [{tableName}];", cancellationToken);
            await ResetSqlServerIdentityIfNeededAsync(tableName, cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    private async Task ResetSqliteAsync(CancellationToken cancellationToken)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        foreach (var tableName in TablesToClearInOrder)
        {
            _log($"Clearing {tableName}...");
            await _db.Database.ExecuteSqlRawAsync($"DELETE FROM \"{tableName}\";", cancellationToken);
        }

        var sqliteIdentityTables = string.Join(
            ", ",
            TablesToClearInOrder.Select(tableName => $"'{tableName}'"));

        await _db.Database.ExecuteSqlRawAsync(
            $"DELETE FROM sqlite_sequence WHERE name IN ({sqliteIdentityTables});",
            cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    private async Task ResetSqlServerIdentityIfNeededAsync(string tableName, CancellationToken cancellationToken)
    {
        var shouldReseed = await _db.Database.SqlQueryRaw<int>($"""
            SELECT COUNT(1)
            FROM sys.identity_columns
            WHERE object_id = OBJECT_ID('{tableName}');
            """).SingleAsync(cancellationToken) > 0;

        if (!shouldReseed)
        {
            return;
        }

        await _db.Database.ExecuteSqlRawAsync($"DBCC CHECKIDENT ('[{tableName}]', RESEED, 0);", cancellationToken);
    }
}
