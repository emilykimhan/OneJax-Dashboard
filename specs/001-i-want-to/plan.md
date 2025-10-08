# Implementation Plan: Database Integration with SQLite

**Branch**: `001-i-want-to` | **Date**: 2025-10-07 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-i-want-to/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Transform OneJax Strategic Dashboard from in-memory data storage to persistent SQLite database using Entity Framework Core. Enable users to create, edit, and delete strategic goals, strategies, and metrics with data persisting across application sessions. Includes configurable database location, export/import functionality, optimistic locking for concurrent access, and comprehensive error handling.

## Technical Context

<!--
  ACTION REQUIRED: Replace the content in this section with the technical details
  for the project. The structure here is presented in advisory capacity to guide
  the iteration process.
-->

**Language/Version**: C# / .NET 9.0  
**Primary Dependencies**: ASP.NET Core MVC, Entity Framework Core, Microsoft.EntityFrameworkCore.Sqlite  
**Storage**: SQLite database with configurable file location  
**Testing**: xUnit for unit testing, ASP.NET Core TestHost for integration testing  
**Target Platform**: Cross-platform web application (Windows, macOS, Linux)
**Project Type**: Single web application with MVC architecture  
**Performance Goals**: Database operations <500ms, application startup increase <2s, UI response <200ms  
**Constraints**: Single SQLite file, optimistic locking for concurrency, basic validation only  
**Scale/Scope**: Small to medium organizations, 100+ strategic goals, 500+ metrics, configurable database location

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Data-Driven Decision Making**: ✅ Feature includes measurable outcomes and traceable data
**User-Centric Design**: ✅ UI elements prioritize clarity and accessibility
**Performance & Responsiveness**: ✅ 2s load time, 200ms interaction response targets met
**Maintainable Architecture**: ✅ Clean separation of concerns, testable business logic
**Security & Privacy**: ✅ Authentication, authorization, and input validation implemented

## Project Structure

### Documentation (this feature)

```
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```
StrategicDashboard/                    # Main ASP.NET Core MVC application
├── Data/
│   ├── DashboardDbContext.cs         # Entity Framework DbContext
│   ├── Configurations/               # Entity configurations
│   └── Migrations/                   # EF Core migration files
├── Models/
│   ├── StrategicGoal.cs             # Enhanced with EF attributes
│   ├── Strategy.cs                   # Enhanced with EF attributes
│   ├── Metric.cs                     # Enhanced with EF attributes
│   ├── EventEntry.cs                 # New entity for event data
│   └── ViewModels/                   # DTOs for requests/responses
│       ├── DashboardViewModel.cs     # Updated for database
│       ├── CreateStrategicGoalRequest.cs
│       └── UpdateStrategicGoalRequest.cs
├── Controllers/
│   ├── HomeController.cs             # Updated for EF Core
│   ├── StrategyController.cs         # Updated for EF Core
│   ├── DataEntryController.cs        # Updated for EF Core
│   └── DataManagementController.cs   # New for export/import
├── Services/
│   ├── DatabaseErrorHandler.cs      # Error handling service
│   ├── DataExportService.cs          # Export functionality
│   └── DataImportService.cs          # Import functionality
├── Views/                            # Existing views updated
├── wwwroot/                          # Existing static files
├── appsettings.json                  # Updated with DB config
└── Program.cs                        # Updated with EF registration

Data/                                  # Database files (created at runtime)
└── OneJaxDashboard.db                # SQLite database file

Tests/                                 # Test project structure
├── Unit/
│   ├── Models/                       # Model validation tests
│   ├── Services/                     # Service logic tests
│   └── Controllers/                  # Controller tests with mocked context
└── Integration/
    ├── DatabaseTests.cs              # EF Core integration tests
    ├── ApiEndpointTests.cs           # End-to-end API tests
    └── ConcurrencyTests.cs           # Optimistic locking tests
```

**Structure Decision**: Single ASP.NET Core MVC application with Entity Framework Core integration. The existing project structure is enhanced with database layer components while maintaining the current MVC architecture. Database files are stored separately from source code for configuration flexibility.

## Complexity Tracking

*No constitutional violations identified. All principles are met:*

- **Data-Driven Decision Making**: ✅ Database ensures data traceability and accuracy
- **User-Centric Design**: ✅ Maintains existing UI patterns with error handling improvements  
- **Performance & Responsiveness**: ✅ Database operations <500ms, startup impact <2s
- **Maintainable Architecture**: ✅ Clean separation with DbContext, services, and validation
- **Security & Privacy**: ✅ Input validation, error handling, and secure data storage

No complexity justifications required.
