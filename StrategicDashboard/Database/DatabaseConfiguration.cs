using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace OneJaxDashboard.Data;

public enum DatabaseProvider
{
    Sqlite,
    SqlServer
}

public sealed record DatabaseSettings(
    DatabaseProvider Provider,
    string ConnectionString,
    bool InitializeSchemaOnStartup,
    bool ApplyMigrationsOnStartup);

public static class DatabaseConfiguration
{
    public static DatabaseSettings Resolve(IConfiguration configuration, string? environmentName = null)
    {
        var providerSetting = configuration["DatabaseProvider"];
        var provider = ResolveProvider(providerSetting, environmentName);
        var initializeSchemaOnStartup =
            configuration.GetValue<bool?>("Database:InitializeSchemaOnStartup") ?? false;
        var applyMigrationsOnStartup =
            configuration.GetValue<bool?>("Database:ApplyMigrationsOnStartup") ?? true;

        return provider switch
        {
            DatabaseProvider.SqlServer => new DatabaseSettings(
                provider,
                GetRequiredConnectionString(
                    configuration,
                    "AzureSqlConnection",
                    "DatabaseProvider is set to SqlServer, but AzureSqlConnection is missing or still contains placeholder values."),
                initializeSchemaOnStartup,
                applyMigrationsOnStartup),

            _ => new DatabaseSettings(
                provider,
                configuration.GetConnectionString("DefaultConnection") ?? "Data Source=StrategicDashboardDB.db",
                initializeSchemaOnStartup,
                applyMigrationsOnStartup)
        };
    }

    public static void Configure(DbContextOptionsBuilder options, DatabaseSettings settings)
    {
        switch (settings.Provider)
        {
            case DatabaseProvider.SqlServer:
                options
                    .ConfigureWarnings(warnings =>
                        warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
                    .UseSqlServer(
                        settings.ConnectionString,
                        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure());
                break;

            default:
                options.UseSqlite(settings.ConnectionString);
                break;
        }
    }

    private static DatabaseProvider ResolveProvider(string? providerSetting, string? environmentName)
    {
        if (!string.IsNullOrWhiteSpace(providerSetting))
        {
            return providerSetting.Trim().ToLowerInvariant() switch
            {
                "sqlite" => DatabaseProvider.Sqlite,
                "sqlserver" => DatabaseProvider.SqlServer,
                "azuresql" => DatabaseProvider.SqlServer,
                _ => throw new InvalidOperationException(
                    $"Unsupported DatabaseProvider '{providerSetting}'. Use 'Sqlite' or 'SqlServer'.")
            };
        }

        return string.Equals(environmentName, Environments.Production, StringComparison.OrdinalIgnoreCase)
            ? DatabaseProvider.SqlServer
            : DatabaseProvider.Sqlite;
    }

    private static string GetRequiredConnectionString(
        IConfiguration configuration,
        string name,
        string missingMessage)
    {
        var connectionString = configuration.GetConnectionString(name);

        if (string.IsNullOrWhiteSpace(connectionString) || ContainsPlaceholders(connectionString))
        {
            throw new InvalidOperationException(missingMessage);
        }

        return connectionString;
    }

    private static bool ContainsPlaceholders(string connectionString) =>
        connectionString.Contains("{server}", StringComparison.OrdinalIgnoreCase) ||
        connectionString.Contains("{database}", StringComparison.OrdinalIgnoreCase) ||
        connectionString.Contains("{username}", StringComparison.OrdinalIgnoreCase) ||
        connectionString.Contains("{password}", StringComparison.OrdinalIgnoreCase);
}
