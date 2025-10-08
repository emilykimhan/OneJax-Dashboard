# Quick Start: Database Integration Implementation

**Feature**: Database Integration with SQLite  
**Created**: 2025-10-07  
**Purpose**: Step-by-step implementation guide for developers

## Prerequisites

- .NET 9.0 SDK installed
- Visual Studio 2022 or VS Code with C# extension
- Basic understanding of Entity Framework Core
- Existing OneJax Strategic Dashboard codebase

## Implementation Steps

### Step 1: Install NuGet Packages

Add the following packages to `StrategicDashboard.csproj`:

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0" />
```

### Step 2: Create DbContext

Create `Data/DashboardDbContext.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using OneJax.StrategicDashboard.Models;

namespace OneJax.StrategicDashboard.Data
{
    public class DashboardDbContext : DbContext
    {
        public DashboardDbContext(DbContextOptions<DashboardDbContext> options) : base(options) { }

        public DbSet<StrategicGoal> StrategicGoals { get; set; }
        public DbSet<Strategy> Strategies { get; set; }
        public DbSet<Metric> Metrics { get; set; }
        public DbSet<EventEntry> EventEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure entities, relationships, and constraints
            ConfigureStrategicGoal(modelBuilder);
            ConfigureStrategy(modelBuilder);
            ConfigureMetric(modelBuilder);
            ConfigureEventEntry(modelBuilder);
        }
    }
}
```

### Step 3: Update Model Classes

Add Entity Framework attributes to existing models:

```csharp
// StrategicGoal.cs
[Table("StrategicGoals")]
public class StrategicGoal
{
    [Key]
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; }

    // Navigation properties
    public virtual ICollection<Strategy> Strategies { get; set; } = new List<Strategy>();
}
```

### Step 4: Configure Services

Update `Program.cs` to register Entity Framework:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configure database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? Path.Combine(builder.Environment.ContentRootPath, "Data", "OneJaxDashboard.db");

builder.Services.AddDbContext<DashboardDbContext>(options =>
    options.UseSqlite($"Data Source={connectionString}"));

// Existing services
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Ensure database is created and migrations applied
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DashboardDbContext>();
    context.Database.EnsureCreated();
}

// Existing middleware
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

### Step 5: Update Configuration

Add to `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data/OneJaxDashboard.db"
  },
  "DatabaseSettings": {
    "FilePath": "Data/OneJaxDashboard.db",
    "AutoMigrate": true,
    "ConnectionTimeout": 30
  }
}
```

### Step 6: Create Initial Migration

Run in Package Manager Console or terminal:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Step 7: Update Controllers

Modify controllers to use Entity Framework:

```csharp
// HomeController.cs
public class HomeController : Controller
{
    private readonly DashboardDbContext _context;

    public HomeController(DashboardDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string goal = null, string status = null)
    {
        var goals = await _context.StrategicGoals
            .Include(g => g.Strategies)
                .ThenInclude(s => s.Metrics)
            .ToListAsync();

        // Apply filters
        if (!string.IsNullOrEmpty(goal))
        {
            goals = goals.Where(g => g.Name == goal).ToList();
        }

        var viewModel = new DashboardViewModel
        {
            StrategicGoals = goals,
            SelectedGoal = goal,
            SelectedStatus = status
        };

        return View(viewModel);
    }
}
```

### Step 8: Add Error Handling

Create `Services/DatabaseErrorHandler.cs`:

```csharp
public class DatabaseErrorHandler
{
    public static string HandleDbUpdateConcurrencyException(DbUpdateConcurrencyException ex)
    {
        return "The data was modified by another user. Please refresh and try again.";
    }

    public static string HandleDbUpdateException(DbUpdateException ex)
    {
        return "An error occurred while saving data. Please try again.";
    }
}
```

### Step 9: Implement Export/Import

Create `Services/DataExportService.cs` and `Services/DataImportService.cs` for backup functionality.

### Step 10: Add Data Validation

Update models with validation attributes:

```csharp
public class CreateStrategicGoalRequest
{
    [Required(ErrorMessage = "Goal name is required")]
    [StringLength(100, ErrorMessage = "Goal name cannot exceed 100 characters")]
    public string Name { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
}
```

## Testing

### Unit Tests

Create tests for:
- DbContext configuration
- Model validation
- Controller actions with mocked context
- Export/import functionality

### Integration Tests

Create tests for:
- Database operations end-to-end
- Migration scenarios
- Concurrency handling
- Error scenarios

## Database Management

### Backup Strategy
- Regular exports via admin interface
- Database file copying for system backups
- Migration rollback procedures

### Performance Monitoring
- Query performance logging
- Database size monitoring
- Connection pool metrics

## Deployment Considerations

1. **Database Path**: Ensure write permissions for application
2. **Migrations**: Run `dotnet ef database update` during deployment
3. **Backup**: Implement regular backup procedures
4. **Monitoring**: Set up health checks for database connectivity

## Troubleshooting

### Common Issues
- **Database locked**: Check for long-running transactions
- **Migration failures**: Verify database permissions and schema state
- **Performance issues**: Check query patterns and indexing
- **Concurrency conflicts**: Implement proper error handling and user feedback

### Debug Commands
```bash
# Check migration status
dotnet ef migrations list

# Generate SQL script for migration
dotnet ef migrations script

# Verify database schema
dotnet ef dbcontext info
```