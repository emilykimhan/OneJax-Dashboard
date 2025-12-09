using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Data;
OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
var builder = WebApplication.CreateBuilder(args);

// Configure database based on environment and provider setting
var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "Sqlite";
var connectionString = databaseProvider.ToLower() switch
{
    "azure" or "sqlserver" => Environment.GetEnvironmentVariable("AZURE_DB_CONNECTION_STRING") 
                              ?? builder.Configuration.GetConnectionString("AzureConnection"),
    _ => builder.Configuration.GetConnectionString("DefaultConnection")
};

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (databaseProvider.ToLower() == "azure" || databaseProvider.ToLower() == "sqlserver")
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        options.UseSqlite(connectionString);
    }
});

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

// Keep ProjectsService for backward compatibility during transition
builder.Services.AddSingleton<OneJaxDashboard.Services.ProjectsService>();

var app = builder.Build();

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