using Microsoft.EntityFrameworkCore;
using VMS.Data;

var builder = WebApplication.CreateBuilder(args);

// ✅ 1. Add the database context FIRST
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
);

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