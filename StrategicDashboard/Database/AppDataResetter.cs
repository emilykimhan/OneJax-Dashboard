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
    private static readonly HashSet<string> SupportedTables = TablesToClearInOrder.ToHashSet(StringComparer.Ordinal);

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
            await _db.Database.ExecuteSqlRawAsync(BuildSqlServerDeleteSql(tableName), cancellationToken);
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
            await _db.Database.ExecuteSqlRawAsync(BuildSqliteDeleteSql(tableName), cancellationToken);
        }

        var sqliteIdentityTables = string.Join(
            ", ",
            TablesToClearInOrder.Select(BuildSqliteStringLiteral));

        await _db.Database.ExecuteSqlRawAsync(
            "DELETE FROM sqlite_sequence WHERE name IN (" + sqliteIdentityTables + ");",
            cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    private async Task ResetSqlServerIdentityIfNeededAsync(string tableName, CancellationToken cancellationToken)
    {
        var validatedTableName = ValidateSupportedTableName(tableName);

        var shouldReseed = await _db.Database.SqlQuery<int>($"""
            SELECT COUNT(1)
            FROM sys.identity_columns
            WHERE object_id = OBJECT_ID({validatedTableName});
            """).SingleAsync(cancellationToken) > 0;

        if (!shouldReseed)
        {
            return;
        }

        await _db.Database.ExecuteSqlRawAsync(BuildSqlServerReseedSql(validatedTableName), cancellationToken);
    }

    private static string BuildSqlServerDeleteSql(string tableName)
    {
        return "DELETE FROM " + QuoteSqlServerIdentifier(tableName) + ";";
    }

    private static string BuildSqliteDeleteSql(string tableName)
    {
        return "DELETE FROM " + QuoteSqliteIdentifier(tableName) + ";";
    }

    private static string BuildSqlServerReseedSql(string tableName)
    {
        return "DBCC CHECKIDENT ('" + QuoteSqlServerIdentifier(tableName) + "', RESEED, 0);";
    }

    private static string QuoteSqlServerIdentifier(string tableName)
    {
        var validatedTableName = ValidateSupportedTableName(tableName);
        return "[" + validatedTableName.Replace("]", "]]", StringComparison.Ordinal) + "]";
    }

    private static string QuoteSqliteIdentifier(string tableName)
    {
        var validatedTableName = ValidateSupportedTableName(tableName);
        return "\"" + validatedTableName.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
    }

    private static string BuildSqliteStringLiteral(string tableName)
    {
        var validatedTableName = ValidateSupportedTableName(tableName);
        return "'" + validatedTableName.Replace("'", "''", StringComparison.Ordinal) + "'";
    }

    private static string ValidateSupportedTableName(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName) || !SupportedTables.Contains(tableName))
        {
            throw new InvalidOperationException($"Unsupported table name '{tableName}'.");
        }

        return tableName;
    }
}
