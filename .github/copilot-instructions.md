# OneJax Strategic Dashboard - AI Coding Assistant Guide

## Project Overview
OneJax Strategic Dashboard is an ASP.NET Core MVC application for tracking and visualizing strategic goals, strategies, and metrics for the OneJax organization. The dashboard allows filtering and viewing of strategic data, as well as entry of new event data.

## Architecture
- **ASP.NET Core MVC** application (targeting .NET 9.0)
- **Model-View-Controller** pattern for separation of concerns
- **In-memory data storage** (no database integration yet)
- **Bootstrap** for frontend styling with custom OneJax branding

## Key Components

### Models
- `StrategicGoal`: Top-level organizational goals (Community Engagement, etc.)
- `Strategy`: Specific strategies belonging to goals
- `Metric`: KPIs tied to strategies with targets, progress, and statuses
- `EventEntryViewModel`: For collecting event assessment data
- `DashboardViewModel`: Aggregates goals data for the dashboard view

### Controllers
- `HomeController`: Main dashboard display with filtering capabilities
- `StrategyController`: Manages strategies and metrics (add/view)
- `DataEntryController`: Handles event data collection

### Views
- `Home/Index`: Main dashboard with goal tabs and metric filtering
- `Strategy/Index`: View/add strategies and metrics
- `DataEntry/Index`: Form for event data collection
- `Shared/_Layout`: Common layout with navigation and styling

## Project Patterns and Conventions

### Data Structure
The application follows a hierarchical data model:
- Strategic Goals contain multiple Strategies
- Strategies contain multiple Metrics
- Example: `Model.StrategicGoals[0].Strategies[0].Metrics[0]`

### Filtering Pattern
Filters are implemented via query parameters and LINQ:
```csharp
// Example from HomeController
var filteredGoals = string.IsNullOrEmpty(goal)
    ? allGoals
    : allGoals.Where(g => g.Name == goal).ToList();

// Metrics filtering
s.Metrics = s.Metrics
    .Where(m => (string.IsNullOrEmpty(status) || m.Status == status)
             && (string.IsNullOrEmpty(time) || m.TimePeriod == time))
    .ToList();
```

### Styling
- Custom color variables defined in `wwwroot/css/dashboard.css`
- OneJax brand colors accessed via CSS variables (e.g., `var(--onejax-navy)`)
- Bootstrap 5.3.2 for responsive layout

## Development Workflow
1. Build the solution with `dotnet build` (or use the "build" task)
2. Run the application with `dotnet run` from the StrategicDashboard directory
3. Access the application at `https://localhost:5001` or `http://localhost:5000`

## Current Limitations
- Uses in-memory data (no database persistence)
- Metrics and strategies need to be manually added
- Export functionality buttons are present but not implemented

## Common Tasks
- To add a new view: Create a .cshtml file in appropriate Views folder and a corresponding controller action
- To add a new controller: Create a class that inherits from Controller in the Controllers folder
- To add a new model: Create a class in the Models folder with appropriate properties

## Code Example: Adding a New Strategic Goal
```csharp
// In HomeController or similar
allGoals.Add(new StrategicGoal
{
    Id = [next available ID],
    Name = "New Goal Name",
    Strategies = new List<Strategy>()
});
```