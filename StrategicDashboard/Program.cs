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

    RunStartupStep("Ensuring staff admin schema support", () => EnsureStaffAdminSupport(db));
    RunStartupStep("Ensuring strategy program schema support", () => EnsureStrategyProgramSupport(db));
    RunStartupStep("Ensuring strategy archive schema support", () => EnsureStrategyArchiveSupport(db));
    RunStartupStep("Ensuring program archive schema support", () => EnsureProgramArchiveSupport(db));
    RunStartupStep("Ensuring activity log schema support", () => EnsureActivityLogSupport(db));
    RunStartupStep("Ensuring fallback admin access", () => EnsureFallbackAdminAccess(db, builder.Configuration));
    RunStartupStep("Ensuring professional development schema support", () => EnsureProfessionalDevelopmentSchemaSupport(db));
    RunStartupStep("Ensuring canonical strategic goals", () => EnsureCanonicalStrategicGoals(db));
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

static void RunStartupStep(string stepName, Action action)
{
    Console.WriteLine($"[startup] {stepName}...");

    try
    {
        action();
        Console.WriteLine($"[startup] {stepName} complete.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[startup] {stepName} failed: {ex}");
    }
}

static void EnsureCanonicalStrategicGoals(ApplicationDbContext db)
{
    if (db.Database.IsSqlServer())
    {
        db.Database.ExecuteSqlRaw("""
            IF EXISTS (
                SELECT 1
                FROM (VALUES (1), (2), (3), (4)) AS required_goals(Id)
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM [StrategicGoals]
                    WHERE [StrategicGoals].[Id] = required_goals.Id
                )
            )
            BEGIN
                SET IDENTITY_INSERT [StrategicGoals] ON;

                IF NOT EXISTS (SELECT 1 FROM [StrategicGoals] WHERE [Id] = 1)
                    INSERT INTO [StrategicGoals] ([Id], [Name], [Description], [Color])
                    VALUES (1, N'Organizational Building', N'', N'');

                IF NOT EXISTS (SELECT 1 FROM [StrategicGoals] WHERE [Id] = 2)
                    INSERT INTO [StrategicGoals] ([Id], [Name], [Description], [Color])
                    VALUES (2, N'Financial Sustainability', N'', N'');

                IF NOT EXISTS (SELECT 1 FROM [StrategicGoals] WHERE [Id] = 3)
                    INSERT INTO [StrategicGoals] ([Id], [Name], [Description], [Color])
                    VALUES (3, N'Identity/Value Proposition', N'', N'');

                IF NOT EXISTS (SELECT 1 FROM [StrategicGoals] WHERE [Id] = 4)
                    INSERT INTO [StrategicGoals] ([Id], [Name], [Description], [Color])
                    VALUES (4, N'Community Engagement', N'', N'');

                SET IDENTITY_INSERT [StrategicGoals] OFF;
            END

            UPDATE [StrategicGoals] SET [Name] = N'Organizational Building' WHERE [Id] = 1;
            UPDATE [StrategicGoals] SET [Name] = N'Financial Sustainability' WHERE [Id] = 2;
            UPDATE [StrategicGoals] SET [Name] = N'Identity/Value Proposition' WHERE [Id] = 3;
            UPDATE [StrategicGoals] SET [Name] = N'Community Engagement' WHERE [Id] = 4;
            """);
    }

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
        // Ensure Staffauth.Id is an IDENTITY column. The table was originally created
        // without IDENTITY (e.g. via SqliteToSqlServerMigrator), which causes EF Core
        // INSERTs to fail because they omit Id and expect the database to generate it.
        var ssConn = db.Database.GetDbConnection();
        var ssConnShouldClose = ssConn.State != ConnectionState.Open;
        if (ssConnShouldClose) ssConn.Open();

        try
        {
            using var checkCmd = ssConn.CreateCommand();
            checkCmd.CommandText = "SELECT ISNULL(COLUMNPROPERTY(OBJECT_ID(N'Staffauth'), N'Id', 'IsIdentity'), -1)";
            var identityFlag = Convert.ToInt32(checkCmd.ExecuteScalar() ?? -1);

            if (identityFlag == 0)
            {
                // Staffauth.Id exists but has no IDENTITY — recreate the table.

                // 1. Drop all FK constraints on other tables that reference Staffauth.
                using (var cmd = ssConn.CreateCommand())
                {
                    cmd.CommandText = """
                        DECLARE @sql NVARCHAR(MAX) = N'';
                        SELECT @sql += N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(fk.parent_object_id)) +
                            '.' + QUOTENAME(OBJECT_NAME(fk.parent_object_id)) +
                            ' DROP CONSTRAINT ' + QUOTENAME(fk.name) + '; '
                        FROM sys.foreign_keys fk
                        WHERE fk.referenced_object_id = OBJECT_ID(N'Staffauth');
                        IF LEN(@sql) > 0 EXEC sp_executesql @sql;
                        """;
                    cmd.ExecuteNonQuery();
                }

                // 2. Drop non-PK indexes on Staffauth.
                using (var cmd = ssConn.CreateCommand())
                {
                    cmd.CommandText = """
                        DECLARE @sql NVARCHAR(MAX) = N'';
                        SELECT @sql += N'DROP INDEX ' + QUOTENAME(i.name) + ' ON [Staffauth]; '
                        FROM sys.indexes i
                        WHERE i.object_id = OBJECT_ID(N'Staffauth')
                            AND i.type > 0
                            AND i.is_primary_key = 0;
                        IF LEN(@sql) > 0 EXEC sp_executesql @sql;
                        """;
                    cmd.ExecuteNonQuery();
                }

                // 3. Remove any leftover backup table from a previous failed attempt.
                using (var cmd = ssConn.CreateCommand())
                {
                    cmd.CommandText = "IF OBJECT_ID(N'Staffauth_Bak', N'U') IS NOT NULL DROP TABLE [Staffauth_Bak];";
                    cmd.ExecuteNonQuery();
                }

                // 4. Rename old table.
                using (var cmd = ssConn.CreateCommand())
                {
                    cmd.CommandText = "EXEC sp_rename N'Staffauth', N'Staffauth_Bak';";
                    cmd.ExecuteNonQuery();
                }

                // 5. Create new table with IDENTITY on Id.
                using (var cmd = ssConn.CreateCommand())
                {
                    cmd.CommandText = """
                        CREATE TABLE [Staffauth] (
                            [Id]       INT           IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Staffauth] PRIMARY KEY,
                            [Name]     NVARCHAR(MAX) NOT NULL DEFAULT (N''),
                            [Username] NVARCHAR(450) NULL,
                            [Password] NVARCHAR(MAX) NULL,
                            [Email]    NVARCHAR(MAX) NOT NULL DEFAULT (N''),
                            [IsAdmin]  BIT           NOT NULL DEFAULT (0)
                        );
                        """;
                    cmd.ExecuteNonQuery();
                }

                // 6. Copy all existing rows, preserving their original Ids.
                using (var cmd = ssConn.CreateCommand())
                {
                    cmd.CommandText = """
                        SET IDENTITY_INSERT [Staffauth] ON;
                        INSERT INTO [Staffauth] ([Id], [Name], [Username], [Password], [Email], [IsAdmin])
                        SELECT [Id],
                               ISNULL([Name], N''),
                               [Username],
                               [Password],
                               ISNULL([Email], N''),
                               ISNULL([IsAdmin], 0)
                        FROM [Staffauth_Bak];
                        SET IDENTITY_INSERT [Staffauth] OFF;
                        """;
                    cmd.ExecuteNonQuery();
                }

                // 7. Drop the backup.
                using (var cmd = ssConn.CreateCommand())
                {
                    cmd.CommandText = "DROP TABLE [Staffauth_Bak];";
                    cmd.ExecuteNonQuery();
                }

                // 8. Recreate the unique index (doubles as the FK principal key for Events.OwnerUsername).
                using (var cmd = ssConn.CreateCommand())
                {
                    cmd.CommandText = "CREATE UNIQUE INDEX [IX_Staffauth_Username] ON [Staffauth] ([Username]) WHERE [Username] IS NOT NULL;";
                    cmd.ExecuteNonQuery();
                }
            }
        }
        finally
        {
            if (ssConnShouldClose) ssConn.Close();
        }

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

static void EnsureFallbackAdminAccess(ApplicationDbContext db, IConfiguration configuration)
{
    try
    {
        if (db.Staffauth.Any(staff => staff.IsAdmin))
        {
            return;
        }

        var fallbackAdmin = db.Staffauth.FirstOrDefault(staff => staff.Username == "admin");
        if (fallbackAdmin == null)
        {
            var bootstrapUsername = configuration["AdminBootstrap:Username"];
            var bootstrapPassword = configuration["AdminBootstrap:Password"];
            var bootstrapEmail = configuration["AdminBootstrap:Email"];
            var bootstrapName = configuration["AdminBootstrap:Name"];

            if (string.IsNullOrWhiteSpace(bootstrapUsername) ||
                string.IsNullOrWhiteSpace(bootstrapPassword) ||
                string.IsNullOrWhiteSpace(bootstrapEmail))
            {
                Console.WriteLine("[admin-bootstrap] No admin accounts found. Set AdminBootstrap__Username, AdminBootstrap__Password, and AdminBootstrap__Email to create the first admin account.");
                return;
            }

            db.Staffauth.Add(new Staffauth
            {
                Username = bootstrapUsername.Trim(),
                Password = bootstrapPassword,
                Email = bootstrapEmail.Trim(),
                Name = string.IsNullOrWhiteSpace(bootstrapName) ? bootstrapUsername.Trim() : bootstrapName.Trim(),
                IsAdmin = true
            });

            db.SaveChanges();
            Console.WriteLine($"[admin-bootstrap] Created first administrator account '{bootstrapUsername.Trim()}' because no admin accounts were found.");
            return;
        }

        fallbackAdmin.IsAdmin = true;
        db.SaveChanges();
        Console.WriteLine("[admin-bootstrap] Promoted 'admin' to administrator because no admin accounts were found.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[admin-bootstrap] Failed to create or promote an administrator: {ex}");
    }
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
        sqlServerDefinition: "int NOT NULL CONSTRAINT [DF_ProfessionalDevelopments_Year] DEFAULT(0)",
        sqliteDefinition: "\"Year\" INTEGER NOT NULL DEFAULT 0");

    EnsureRequiredColumn(
        db,
        tableName: "ProfessionalDevelopments",
        columnName: "Activities",
        sqlServerDefinition: "nvarchar(2000) NOT NULL CONSTRAINT [DF_ProfessionalDevelopments_Activities] DEFAULT(N'')",
        sqliteDefinition: "\"Activities\" TEXT NOT NULL DEFAULT ''");

    EnsureRequiredColumn(
        db,
        tableName: "ProfessionalDevelopments",
        columnName: "Month",
        sqlServerDefinition: "nvarchar(20) NOT NULL CONSTRAINT [DF_ProfessionalDevelopments_Month] DEFAULT(N'')",
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
        var tableIdentifier = DelimitSqlServerIdentifier(tableName);
        var columnIdentifier = DelimitSqlServerIdentifier(columnName);
        var tableNameLiteral = EscapeSqlServerStringLiteral(tableName);
        var columnNameLiteral = EscapeSqlServerStringLiteral(columnName);
        var sql = $"""
            IF COL_LENGTH('{tableNameLiteral}', '{columnNameLiteral}') IS NULL
            BEGIN
                ALTER TABLE {tableIdentifier}
                ADD {columnIdentifier} {sqlServerDefinition};
            END
            """;

        db.Database.ExecuteSqlRaw(sql);

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

static string DelimitSqlServerIdentifier(string identifier)
{
    if (string.IsNullOrWhiteSpace(identifier) ||
        identifier.Any(character => !char.IsLetterOrDigit(character) && character != '_'))
    {
        throw new InvalidOperationException($"'{identifier}' is not a valid SQL Server identifier.");
    }

    return $"[{identifier}]";
}

static string EscapeSqlServerStringLiteral(string value) => value.Replace("'", "''", StringComparison.Ordinal);

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
