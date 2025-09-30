var builder = WebApplication.CreateBuilder(args);

// Addings this service for controllers with views
builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseStaticFiles();

// Use routing and endpoints for controllers with views
app.UseRouting();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
