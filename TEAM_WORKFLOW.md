# OneJax Team Development Workflow

## Branch Strategy
```
main (production)
├── feature/enhanced-event-tracking (Emily - Dashboard core)
├── feature/event-management-forms (Teammate 2)
├── feature/financial-tracking (Teammate 3)
└── feature/community-engagement (Teammate 4)
```

## File Ownership (Minimize Conflicts)

## Team Member Responsibilities

### Emily (Dashboard Integration Lead):
- **Files to focus on**: 
  - `Controllers/HomeController.cs` 
  - `Views/Home/` 
  - `Controllers/DashboardApiController.cs`
  - `Services/MockDataService.cs`
  - `wwwroot/css/` (shared styling)

### Team Member 1 (Account Management):
- **Files to focus on**:
  - `Controllers/AccountController.cs`
  - `Views/Account/`
  - `Models/LoginViewModel.cs`, `ChangePasswordViewModel.cs`
  - Authentication middleware and user management

### Team Member 2 (Event Management):
- **Files to focus on**:
  - `Controllers/EventsController.cs`
  - `Views/Events/`
  - Event CRUD operations and calendar features
  - Event status tracking and notifications

### Team Member 3 (Forms/Metrics Management):
- **Files to focus on**:
  - `Controllers/DataEntryController.cs`
  - `Controllers/StaffSurveyController.cs`
  - `Views/DataEntry/`, form views
  - Metrics calculation and data validation

## Integration Points

### Shared Models (Coordinate Changes):
```csharp
// Everyone uses these - discuss changes in team chat
- Models/Event.cs
- Models/GoalMetric.cs  
- Models/StrategicGoal.cs
- Database/AppDbContext.cs
```

### Integration Methods:
1. **Direct DB writes** - Fastest, works immediately
2. **API calls** - More structured, better for complex data
3. **Service injection** - Cleanest, but requires coordination

## Daily Workflow
1. **Morning**: Pull latest from main
2. **Work**: In your feature branch
3. **Evening**: Push your feature branch
4. **Integration**: Test locally with `dotnet run`
5. **PR Review**: Before merging to main

## Avoiding Conflicts

### Safe to Edit Independently:
- Your own Controllers/
- Your own Views/
- Your own wwwroot/js/ files
- README files
- Config files (appsettings.json sections)

### Coordinate Before Editing:
- Models/ (shared data structures)
- Database/AppDbContext.cs
- Program.cs (service registrations)
- Shared CSS files

## Testing Integration
```bash
# Each person can test locally
git checkout main
git pull origin main
git checkout your-feature-branch
git merge main  # Resolve any conflicts
dotnet run      # Test everything works together
```
