using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Data; 
using OneJaxDashboard.Models;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
var builder = WebApplication.CreateBuilder(args);

var databaseSettings = DatabaseConfiguration.Resolve(builder.Configuration, builder.Environment.EnvironmentName);
Console.WriteLine($"[startup] Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"[startup] Database provider: {databaseSettings.Provider}");
var runSqliteMigration = args.Contains("--migrate-sqlite-to-sqlserver", StringComparer.OrdinalIgnoreCase);
var runAdminCountCheck = args.Contains("--check-admin-count", StringComparer.OrdinalIgnoreCase);
var runAppDataReset = args.Contains("--reset-app-data", StringComparer.OrdinalIgnoreCase);

if (runSqliteMigration)
{
    if (databaseSettings.Provider != DatabaseProvider.SqlServer)
    {
        throw new InvalidOperationException(
            "The SQLite migration command requires DatabaseProvider=SqlServer for the target database.");
    }

    var sourceSqliteConnection =
        builder.Configuration.GetConnectionString("SqliteMigrationSource")
        ?? builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=StrategicDashboardDB.db";

    var migrator = new SqliteToSqlServerMigrator(
        sourceSqliteConnection,
        databaseSettings.ConnectionString,
        message => Console.WriteLine($"[sqlite-migration] {message}"));

    await migrator.RunAsync();
    return;
}

if (runAdminCountCheck)
{
    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
    DatabaseConfiguration.Configure(optionsBuilder, databaseSettings);

    await using var db = new ApplicationDbContext(optionsBuilder.Options);
    Console.WriteLine(await db.Staffauth.CountAsync(staff => staff.IsAdmin));
    return;
}

if (runAppDataReset)
{
    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
    DatabaseConfiguration.Configure(optionsBuilder, databaseSettings);

    await using var db = new ApplicationDbContext(optionsBuilder.Options);
    var resetter = new AppDataResetter(db, message => Console.WriteLine($"[app-data-reset] {message}"));
    await resetter.RunAsync();
    EnsureCanonicalStrategicGoals(db);
    Console.WriteLine("[app-data-reset] Completed. Staff accounts were preserved.");
    return;
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    DatabaseConfiguration.Configure(options, databaseSettings));

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

builder.Services.AddSingleton<OneJaxDashboard.Services.StaffService>();
builder.Services.AddScoped<OneJaxDashboard.Services.EventsService>();
builder.Services.AddScoped<OneJaxDashboard.Services.StrategyService>();
builder.Services.AddScoped<OneJaxDashboard.Services.ActivityLogService>();
builder.Services.AddScoped<OneJaxDashboard.Services.MetricsService>();
builder.Services.AddSingleton<OneJaxDashboard.Services.IDashboardNotesStore, OneJaxDashboard.Services.DashboardNotesStore>();

// Keep ProjectsService for backward compatibility during transition
builder.Services.AddSingleton<OneJaxDashboard.Services.ProjectsService>();

var easternTimeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Eastern Standard Time" : "America/New_York";
builder.Services.AddSingleton(TimeZoneInfo.FindSystemTimeZoneById(easternTimeZoneId));
Console.WriteLine("[startup] Building web application...");
var app = builder.Build();
Console.WriteLine("[startup] Web application built.");

using (var scope = app.Services.CreateScope())
{
    Console.WriteLine("[startup] Running database bootstrap...");
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (databaseSettings.InitializeSchemaOnStartup)
    {
        Console.WriteLine("[startup] Ensuring schema is created...");
        db.Database.EnsureCreated();
    }

    if (databaseSettings.ApplyMigrationsOnStartup)
    {
        Console.WriteLine("[startup] Applying pending database migrations...");
        db.Database.Migrate();
    }
    else
    {
        Console.WriteLine("[startup] Skipping automatic database migrations.");
    }

    EnsureStaffAdminSupport(db);
    EnsureStrategyProgramSupport(db);
    EnsureStrategyArchiveSupport(db);
    EnsureProgramArchiveSupport(db);
    EnsureActivityLogSupport(db);
    Console.WriteLine("[startup] Ensuring fallback admin access...");
    EnsureFallbackAdminAccess(db);
    EnsureProfessionalDevelopmentSchemaSupport(db);

    Console.WriteLine("[startup] Ensuring canonical strategic goals...");
    EnsureCanonicalStrategicGoals(db);
    Console.WriteLine("[startup] Database bootstrap complete.");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

Console.WriteLine("[startup] Starting web host...");
app.Run();

static void EnsureCanonicalStrategicGoals(ApplicationDbContext db)
{
    var defaultGoals = new[]
    {
        new { Id = 1, Name = "Organizational Building" },
        new { Id = 2, Name = "Financial Sustainability" },
        new { Id = 3, Name = "Identity/Value Proposition" },
        new { Id = 4, Name = "Community Engagement" }
    };

    var canonicalGoalIds = defaultGoals
        .Select(goal => goal.Id)
        .ToHashSet();

    foreach (var goal in defaultGoals)
    {
        var existingGoal = db.StrategicGoals.FirstOrDefault(g => g.Id == goal.Id);
        if (existingGoal == null)
        {
            db.StrategicGoals.Add(new StrategicGoal
            {
                Id = goal.Id,
                Name = goal.Name
            });

            continue;
        }

        if (!string.Equals(existingGoal.Name, goal.Name, StringComparison.Ordinal))
        {
            existingGoal.Name = goal.Name;
        }
    }

    var removableGoals = db.StrategicGoals
        .Where(goal => !canonicalGoalIds.Contains(goal.Id))
        .Where(goal =>
            !db.Events.Any(e =>
                e.StrategyId.HasValue &&
                db.Strategies.Any(s => s.Id == e.StrategyId.Value && s.StrategicGoalId == goal.Id)) &&
            !db.Strategies.Any(s => s.StrategicGoalId == goal.Id) &&
            !db.GoalMetrics.Any(m => m.StrategicGoalId == goal.Id))
        .ToList();

    if (removableGoals.Any())
    {
        db.StrategicGoals.RemoveRange(removableGoals);
    }

    db.SaveChanges();
}

static void EnsureStaffAdminSupport(ApplicationDbContext db)
{
    if (db.Database.IsSqlServer())
    {
        db.Database.ExecuteSqlRaw("""
            IF COL_LENGTH('Staffauth', 'IsAdmin') IS NULL
            BEGIN
                ALTER TABLE [Staffauth]
                ADD [IsAdmin] bit NOT NULL CONSTRAINT [DF_Staffauth_IsAdmin] DEFAULT(0);
            END
            """);

        return;
    }

    if (!db.Database.IsSqlite())
    {
        return;
    }

    var connection = db.Database.GetDbConnection();
    var shouldClose = connection.State != ConnectionState.Open;
    if (shouldClose)
    {
        connection.Open();
    }

    try
    {
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA table_info('Staffauth');";

        var hasIsAdminColumn = false;
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                if (string.Equals(reader["name"]?.ToString(), "IsAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    hasIsAdminColumn = true;
                    break;
                }
            }
        }

        if (!hasIsAdminColumn)
        {
            using var alterCommand = connection.CreateCommand();
            alterCommand.CommandText = """
                ALTER TABLE "Staffauth"
                ADD COLUMN "IsAdmin" INTEGER NOT NULL DEFAULT 0;
                """;
            alterCommand.ExecuteNonQuery();
        }
    }
    finally
    {
        if (shouldClose)
        {
            connection.Close();
        }
    }
}

static void EnsureFallbackAdminAccess(ApplicationDbContext db)
{
    if (db.Staffauth.Any(staff => staff.IsAdmin))
    {
        return;
    }

    var fallbackAdmin = db.Staffauth.FirstOrDefault(staff => staff.Username == "admin");
    if (fallbackAdmin == null)
    {
        return;
    }

    fallbackAdmin.IsAdmin = true;
    db.SaveChanges();
    Console.WriteLine("[admin-bootstrap] Promoted 'admin' to administrator because no admin accounts were found.");
}

static void EnsureProgramArchiveSupport(ApplicationDbContext db)
{
    if (db.Database.IsSqlServer())
    {
        db.Database.ExecuteSqlRaw("""
            IF OBJECT_ID(N'dbo.Programs', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[Programs]
                (
                    [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Programs] PRIMARY KEY,
                    [ProgramName] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_Programs_ProgramName] DEFAULT(N''),
                    [Description] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_Programs_Description] DEFAULT(N''),
                    [ProgramType] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_Programs_ProgramType] DEFAULT(N'')
                );
            END

            IF COL_LENGTH('dbo.Programs', 'Description') IS NULL
            BEGIN
                ALTER TABLE [dbo].[Programs]
                ADD [Description] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_Programs_Description] DEFAULT(N'');
            END

            IF COL_LENGTH('dbo.Programs', 'ProgramType') IS NULL
            BEGIN
                ALTER TABLE [dbo].[Programs]
                ADD [ProgramType] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_Programs_ProgramType] DEFAULT(N'');
            END

            IF OBJECT_ID(N'dbo.ArchivedPrograms', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[ArchivedPrograms]
                (
                    [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_ArchivedPrograms] PRIMARY KEY,
                    [OriginalProgramId] INT NOT NULL CONSTRAINT [DF_ArchivedPrograms_OriginalProgramId] DEFAULT(0),
                    [ProgramName] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_ArchivedPrograms_ProgramName] DEFAULT(N''),
                    [ProgramType] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_ArchivedPrograms_ProgramType] DEFAULT(N''),
                    [Description] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_ArchivedPrograms_Description] DEFAULT(N''),
                    [ArchivedAtUtc] DATETIME2 NOT NULL CONSTRAINT [DF_ArchivedPrograms_ArchivedAtUtc] DEFAULT(SYSUTCDATETIME())
                );
            END

            IF COL_LENGTH('dbo.ArchivedPrograms', 'Description') IS NULL
            BEGIN
                ALTER TABLE [dbo].[ArchivedPrograms]
                ADD [Description] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_ArchivedPrograms_Description] DEFAULT(N'');
            END

            IF COL_LENGTH('dbo.ArchivedPrograms', 'ArchivedAtUtc') IS NULL
            BEGIN
                ALTER TABLE [dbo].[ArchivedPrograms]
                ADD [ArchivedAtUtc] DATETIME2 NOT NULL CONSTRAINT [DF_ArchivedPrograms_ArchivedAtUtc] DEFAULT(SYSUTCDATETIME());
            END
            """);

        return;
    }

    if (!db.Database.IsSqlite())
    {
        return;
    }

    var connection = db.Database.GetDbConnection();
    var shouldClose = connection.State != ConnectionState.Open;
    if (shouldClose)
    {
        connection.Open();
    }

    try
    {
        EnsureSqliteTable(connection, "Programs", """
            CREATE TABLE "Programs" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_Programs" PRIMARY KEY AUTOINCREMENT,
                "ProgramName" TEXT NOT NULL,
                "Description" TEXT NOT NULL DEFAULT '',
                "ProgramType" TEXT NOT NULL
            );
            """);

        EnsureSqliteColumn(connection, "Programs", "Description",
            "ALTER TABLE \"Programs\" ADD COLUMN \"Description\" TEXT NOT NULL DEFAULT '';");
        EnsureSqliteColumn(connection, "Programs", "ProgramType",
            "ALTER TABLE \"Programs\" ADD COLUMN \"ProgramType\" TEXT NOT NULL DEFAULT '';");

        EnsureSqliteTable(connection, "ArchivedPrograms", """
            CREATE TABLE "ArchivedPrograms" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_ArchivedPrograms" PRIMARY KEY AUTOINCREMENT,
                "OriginalProgramId" INTEGER NOT NULL,
                "ProgramName" TEXT NOT NULL,
                "ProgramType" TEXT NOT NULL,
                "Description" TEXT NOT NULL DEFAULT '',
                "ArchivedAtUtc" TEXT NOT NULL
            );
            """);

        EnsureSqliteColumn(connection, "ArchivedPrograms", "Description",
            "ALTER TABLE \"ArchivedPrograms\" ADD COLUMN \"Description\" TEXT NOT NULL DEFAULT '';");
        EnsureSqliteColumn(connection, "ArchivedPrograms", "ArchivedAtUtc",
            "ALTER TABLE \"ArchivedPrograms\" ADD COLUMN \"ArchivedAtUtc\" TEXT NOT NULL DEFAULT '0001-01-01T00:00:00.0000000Z';");
    }
    finally
    {
        if (shouldClose)
        {
            connection.Close();
        }
    }
}

static void EnsureStrategyArchiveSupport(ApplicationDbContext db)
{
    if (db.Database.IsSqlServer())
    {
        db.Database.ExecuteSqlRaw("""
            IF COL_LENGTH('dbo.Strategies', 'IsArchived') IS NULL
            BEGIN
                ALTER TABLE [dbo].[Strategies]
                ADD [IsArchived] bit NOT NULL CONSTRAINT [DF_Strategies_IsArchived] DEFAULT(0);
            END

            IF COL_LENGTH('dbo.Strategies', 'ArchivedAtUtc') IS NULL
            BEGIN
                ALTER TABLE [dbo].[Strategies]
                ADD [ArchivedAtUtc] datetime2 NULL;
            END
            """);

        return;
    }

    if (!db.Database.IsSqlite())
    {
        return;
    }

    var connection = db.Database.GetDbConnection();
    var shouldClose = connection.State != ConnectionState.Open;
    if (shouldClose)
    {
        connection.Open();
    }

    try
    {
        EnsureSqliteColumn(connection, "Strategies", "IsArchived",
            "ALTER TABLE \"Strategies\" ADD COLUMN \"IsArchived\" INTEGER NOT NULL DEFAULT 0;");
        EnsureSqliteColumn(connection, "Strategies", "ArchivedAtUtc",
            "ALTER TABLE \"Strategies\" ADD COLUMN \"ArchivedAtUtc\" TEXT NULL;");
    }
    finally
    {
        if (shouldClose)
        {
            connection.Close();
        }
    }
}

static void EnsureProfessionalDevelopmentSchemaSupport(ApplicationDbContext db)
{
    EnsureRequiredColumn(
        db,
        tableName: "ProfessionalDevelopments",
        columnName: "Year",
        sqlServerDefinition: "[Year] int NOT NULL CONSTRAINT [DF_ProfessionalDevelopments_Year] DEFAULT(0)",
        sqliteDefinition: "\"Year\" INTEGER NOT NULL DEFAULT 0");

    EnsureRequiredColumn(
        db,
        tableName: "ProfessionalDevelopments",
        columnName: "Activities",
        sqlServerDefinition: "[Activities] nvarchar(2000) NOT NULL CONSTRAINT [DF_ProfessionalDevelopments_Activities] DEFAULT(N'')",
        sqliteDefinition: "\"Activities\" TEXT NOT NULL DEFAULT ''");

    EnsureRequiredColumn(
        db,
        tableName: "ProfessionalDevelopments",
        columnName: "Month",
        sqlServerDefinition: "[Month] nvarchar(20) NOT NULL CONSTRAINT [DF_ProfessionalDevelopments_Month] DEFAULT(N'')",
        sqliteDefinition: "\"Month\" TEXT NOT NULL DEFAULT ''");
}

static void EnsureRequiredColumn(
    ApplicationDbContext db,
    string tableName,
    string columnName,
    string sqlServerDefinition,
    string sqliteDefinition)
{
    if (db.Database.IsSqlServer())
    {
        db.Database.ExecuteSqlRaw($"""
            IF COL_LENGTH('{tableName}', '{columnName}') IS NULL
            BEGIN
                ALTER TABLE [{tableName}]
                ADD {sqlServerDefinition};
            END
            """);

        return;
    }

    if (!db.Database.IsSqlite())
    {
        return;
    }

    var connection = db.Database.GetDbConnection();
    var shouldClose = connection.State != ConnectionState.Open;
    if (shouldClose)
    {
        connection.Open();
    }

    try
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info('{tableName}');";

        var hasColumn = false;
        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                if (string.Equals(reader["name"]?.ToString(), columnName, StringComparison.OrdinalIgnoreCase))
                {
                    hasColumn = true;
                    break;
                }
            }
        }

        if (!hasColumn)
        {
            using var alterCommand = connection.CreateCommand();
            alterCommand.CommandText = $"""
                ALTER TABLE "{tableName}"
                ADD COLUMN {sqliteDefinition};
                """;
            alterCommand.ExecuteNonQuery();
        }
    }
    finally
    {
        if (shouldClose)
        {
            connection.Close();
        }
    }
}

static void EnsureStrategyProgramSupport(ApplicationDbContext db)
{
    if (db.Database.IsSqlServer())
    {
        db.Database.ExecuteSqlRaw("""
            IF COL_LENGTH('dbo.Strategies', 'ProgramId') IS NULL
            BEGIN
                ALTER TABLE [dbo].[Strategies]
                ADD [ProgramId] int NULL;
            END

            IF COL_LENGTH('dbo.Strategies', 'ProgramName') IS NULL
            BEGIN
                ALTER TABLE [dbo].[Strategies]
                ADD [ProgramName] nvarchar(max) NULL;
            END

            IF COL_LENGTH('dbo.Strategies', 'ProgramType') IS NULL
            BEGIN
                ALTER TABLE [dbo].[Strategies]
                ADD [ProgramType] nvarchar(max) NULL;
            END
            """);

        return;
    }

    if (!db.Database.IsSqlite())
    {
        return;
    }

    var connection = db.Database.GetDbConnection();
    var shouldClose = connection.State != ConnectionState.Open;
    if (shouldClose)
    {
        connection.Open();
    }

    try
    {
        EnsureSqliteColumn(connection, "Strategies", "ProgramId",
            "ALTER TABLE \"Strategies\" ADD COLUMN \"ProgramId\" INTEGER NULL;");
        EnsureSqliteColumn(connection, "Strategies", "ProgramName",
            "ALTER TABLE \"Strategies\" ADD COLUMN \"ProgramName\" TEXT NULL;");
        EnsureSqliteColumn(connection, "Strategies", "ProgramType",
            "ALTER TABLE \"Strategies\" ADD COLUMN \"ProgramType\" TEXT NULL;");
    }
    finally
    {
        if (shouldClose)
        {
            connection.Close();
        }
    }
}

static void EnsureActivityLogSupport(ApplicationDbContext db)
{
    if (db.Database.IsSqlServer())
    {
        db.Database.ExecuteSqlRaw("""
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

        return;
    }

    if (!db.Database.IsSqlite())
    {
        return;
    }

    var connection = db.Database.GetDbConnection();
    var shouldClose = connection.State != ConnectionState.Open;
    if (shouldClose)
    {
        connection.Open();
    }

    try
    {
        EnsureSqliteTable(connection, "ActivityLogs", """
            CREATE TABLE "ActivityLogs" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_ActivityLogs" PRIMARY KEY AUTOINCREMENT,
                "Timestamp" TEXT NOT NULL,
                "User" TEXT NOT NULL,
                "Action" TEXT NOT NULL,
                "Entity" TEXT NULL,
                "Details" TEXT NULL
            );
            """);

        EnsureSqliteColumn(connection, "ActivityLogs", "Entity",
            "ALTER TABLE \"ActivityLogs\" ADD COLUMN \"Entity\" TEXT NULL;");
        EnsureSqliteColumn(connection, "ActivityLogs", "Details",
            "ALTER TABLE \"ActivityLogs\" ADD COLUMN \"Details\" TEXT NULL;");
    }
    finally
    {
        if (shouldClose)
        {
            connection.Close();
        }
    }
}

static void EnsureSqliteTable(DbConnection connection, string tableName, string createSql)
{
    using var existsCommand = connection.CreateCommand();
    existsCommand.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $name;";

    var nameParameter = existsCommand.CreateParameter();
    nameParameter.ParameterName = "$name";
    nameParameter.Value = tableName;
    existsCommand.Parameters.Add(nameParameter);

    var exists = Convert.ToInt32(existsCommand.ExecuteScalar()) > 0;
    if (exists)
    {
        return;
    }

    using var createCommand = connection.CreateCommand();
    createCommand.CommandText = createSql;
    createCommand.ExecuteNonQuery();
}

static void EnsureSqliteColumn(DbConnection connection, string tableName, string columnName, string alterSql)
{
    using var pragmaCommand = connection.CreateCommand();
    pragmaCommand.CommandText = $"PRAGMA table_info('{tableName}');";

    var hasColumn = false;
    using (var reader = pragmaCommand.ExecuteReader())
    {
        while (reader.Read())
        {
            if (string.Equals(reader["name"]?.ToString(), columnName, StringComparison.OrdinalIgnoreCase))
            {
                hasColumn = true;
                break;
            }
        }
    }

    if (hasColumn)
    {
        return;
    }

    using var alterCommand = connection.CreateCommand();
    alterCommand.CommandText = alterSql;
    alterCommand.ExecuteNonQuery();
}
