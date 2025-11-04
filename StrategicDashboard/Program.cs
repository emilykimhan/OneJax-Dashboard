var builder = WebApplication.CreateBuilder(args);

// ✅ 2. Add MVC controllers with views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ✅ 3. Middleware pipeline
app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();