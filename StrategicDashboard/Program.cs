using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OneJaxDashboard.Data; 
using OneJaxDashboard.Models;
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
            !db.Events.Any(e => e.StrategicGoalId == goal.Id) &&
            !db.Strategies.Any(s => s.StrategicGoalId == goal.Id) &&
            !db.GoalMetrics.Any(m => m.StrategicGoalId == goal.Id))
        .ToList();

    if (removableGoals.Any())
    {
        db.StrategicGoals.RemoveRange(removableGoals);
    }

    db.SaveChanges();
}
