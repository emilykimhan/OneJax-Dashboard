using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OneJaxDashboard.Data; 
using System.Runtime.InteropServices;
OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
var builder = WebApplication.CreateBuilder(args);

// Configure database based on environment
if (builder.Environment.IsProduction())
{
    // Use Azure SQL Database in production
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options
            .UseSqlServer(builder.Configuration.GetConnectionString("AzureSqlConnection"))
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));
}
else
{
    // Use SQLite for development
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options
            .UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));
}

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
builder.Services.AddSingleton<OneJaxDashboard.Services.ActivityLogService>();
builder.Services.AddScoped<OneJaxDashboard.Services.MetricsService>();

// Keep ProjectsService for backward compatibility during transition
builder.Services.AddSingleton<OneJaxDashboard.Services.ProjectsService>();

var easternTimeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Eastern Standard Time" : "America/New_York";
builder.Services.AddSingleton(TimeZoneInfo.FindSystemTimeZoneById(easternTimeZoneId));
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    EnsureStrategicGoalsExist(db);
    // NOTE: destructive DB cleanup should never run implicitly.
    // Opt-in only (Development + ONEJAX_PURGE_DEMO_DATA=1) to remove previously inserted demo rows.
    if (app.Environment.IsDevelopment()
        && string.Equals(Environment.GetEnvironmentVariable("ONEJAX_PURGE_DEMO_DATA"), "1", StringComparison.Ordinal))
    {
        PurgeDemoSeedData(db);
    }
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

static void EnsureStrategicGoalsExist(ApplicationDbContext db)
{
    var defaultGoals = new[]
    {
        new { Id = 1, Name = "Organizational Building" },
        new { Id = 2, Name = "Financial Sustainability" },
        new { Id = 3, Name = "Identity/Value Proposition" },
        new { Id = 4, Name = "Community Engagement" }
    };

    foreach (var goal in defaultGoals)
    {
        if (!db.StrategicGoals.Any(g => g.Id == goal.Id))
        {
            db.StrategicGoals.Add(new OneJaxDashboard.Models.StrategicGoal
            {
                Id = goal.Id,
                Name = goal.Name
            });
        }
    }

    db.SaveChanges();
}

static void PurgeDemoSeedData(ApplicationDbContext db)
{
    // Keep only the four canonical strategic goals. Old demo/duplicate goal rows create extra tabs.
    var extraGoals = db.StrategicGoals
        .Where(g => g.Id < 1 || g.Id > 4)
        .ToList();
    if (extraGoals.Count > 0)
    {
        db.StrategicGoals.RemoveRange(extraGoals);
        db.SaveChanges();
    }

    // Remove duplicated/demo Events that were previously inserted (these pollute the Events UI).
    // Delete events first to avoid FK issues when removing strategy templates.
    var demoEventTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "February Interfaith Dialogue",
        "Community Outreach - Downtown",
        "Cross-Cultural Workshop Series",
        "Q1 Strategic Planning Session",
        "Leadership Development Kickoff"
    };

    var demoEvents = db.Events
        .Where(e => e.OwnerUsername == "admin" && demoEventTitles.Contains(e.Title))
        .ToList();
    if (demoEvents.Count > 0)
    {
        db.Events.RemoveRange(demoEvents);
        db.SaveChanges();
    }

    // Remove duplicated/demo Strategies (these show up as "fake events" in /Strategy/ViewEvents).
    // Heuristic: seeded strategies have no program linkage/type/partners/cross-collab, but do have a name+date+time.
    var demoStrategyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Interfaith Dialogue Series",
        "Community Outreach Program",
        "Cross-Cultural Workshops",
        "Humanitarian Relief Initiative",
        "Strategic Planning Framework",
        "Leadership Development Program",
        "Organizational Assessment",
        "Partnership Development",
        "Staff Training Workshops",
        "Mentorship Program",
        "Conference Attendance",
        "Skills Assessment",
        "Grant Writing Initiative",
        "Fee-for-Service Programs",
        "Donor Engagement Campaign",
        "Corporate Partnership Program"
    };

    var demoStrategies = db.Strategies
        .Where(s =>
            demoStrategyNames.Contains(s.Name) &&
            s.ProgramId == null &&
            s.ProgramName == null &&
            s.ProgramType == null &&
            (s.Partners ?? "") == "" &&
            (s.CrossCollaboration ?? "") == "" &&
            s.Date != null &&
            s.Time != null)
        .ToList();
    if (demoStrategies.Count > 0)
    {
        db.Strategies.RemoveRange(demoStrategies);
        db.SaveChanges();
    }
}
