using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Data; 
using OneJaxDashboard.Models;
using System.Data;
using System.Runtime.InteropServices;
OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
var builder = WebApplication.CreateBuilder(args);

var databaseSettings = DatabaseConfiguration.Resolve(builder.Configuration, builder.Environment.EnvironmentName);
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
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (databaseSettings.InitializeSchemaOnStartup)
    {
        db.Database.EnsureCreated();
    }

    EnsureStaffAdminSupport(db);

    EnsureCanonicalStrategicGoals(db);
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
