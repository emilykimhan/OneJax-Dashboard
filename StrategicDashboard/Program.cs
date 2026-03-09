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
