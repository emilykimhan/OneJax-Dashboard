# OneJax Strategic Dashboard

A web-based strategic dashboard built for OneJax to track organizational metrics, community engagement, events, staff activity, and more. Built with ASP.NET Core 9 MVC and supports both SQLite (local development) and Azure SQL Server (production).

---

## Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Local Development Setup](#local-development-setup)
- [Database Configuration](#database-configuration)
- [Running the App](#running-the-app)
- [CLI Utilities](#cli-utilities)
- [Deployment](#deployment)
- [Project Structure](#project-structure)

---

## Features

- **Strategic Metrics Dashboard** — Track goal metrics, KPIs, and performance trends
- **Community & Donor Engagement** — Log and visualize engagement activity
- **Events Management** — Track events including interfaith events
- **Staff Portal** — Staff authentication, surveys, and management
- **Financial Tracking** — Financial data entry and reporting
- **Media Placements** — Log and review media coverage
- **Professional Development** — Track staff learning and development
- **Programs & Organizational Building** — Monitor programs and org health
- **Website Traffic** — Record and display website analytics
- **Data Export** — Export data to Excel (via ClosedXML / EPPlus)
- **Admin Controls** — Metrics admin panel and activity logging
- **Role-based Access** — Cookie authentication with admin and staff roles

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 9 MVC |
| ORM | Entity Framework Core 9 |
| Local Database | SQLite |
| Production Database | Azure SQL Server |
| Auth | ASP.NET Core Cookie Authentication |
| Excel Export | ClosedXML, EPPlus |
| Hosting | Azure App Service (Linux) |
| IaC | Azure ARM Templates |

---

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Git](https://git-scm.com/)
- (Optional, for production) An Azure subscription with an Azure SQL Server and App Service

---

## Local Development Setup

1. **Clone the repository**

   ```bash
   git clone <repo-url>
   cd OneJax-Dashboard
   ```

2. **Restore dependencies**

   ```bash
   dotnet restore StrategicDashboard/StrategicDashboard.csproj
   ```

3. **Configure local settings**

   The default `appsettings.json` uses SQLite with a local `StrategicDashboardDB.db` file — no extra configuration needed for local development.

   If you need to override settings, create `appsettings.Development.json` in the `StrategicDashboard/` folder.

4. **Apply database migrations**

   ```bash
   cd StrategicDashboard
   dotnet ef database update
   ```

5. **Run the app**

   ```bash
   dotnet run
   ```

   The app will be available at `https://localhost:5001` (or the port shown in the terminal).

---

## Database Configuration

The app supports two database providers, selected via `appsettings.json`:

| Setting | Value | Description |
|---|---|---|
| `DatabaseProvider` | `Sqlite` | Uses a local `.db` file (default for dev) |
| `DatabaseProvider` | `SqlServer` | Uses Azure SQL Server (production) |

### SQLite (Development)

No extra setup. The connection string defaults to:
```
Data Source=StrategicDashboardDB.db
```

### Azure SQL Server (Production)

Set the `DatabaseProvider` to `SqlServer` and provide a connection string. In production, this is injected via an Azure App Service connection string environment variable or GitHub Actions secret — **never commit real credentials**.

The production `appsettings.Production.json` sets `ApplyMigrationsOnStartup: true`, so EF Core migrations run automatically on startup.

---

## Running the App

```bash
cd StrategicDashboard
dotnet run
```

To run in a specific environment:

```bash
ASPNETCORE_ENVIRONMENT=Production dotnet run
```

---

## CLI Utilities

The app exposes several one-off CLI commands via startup arguments:

| Command | Description |
|---|---|
| `--migrate-sqlite-to-sqlserver` | Migrates data from a local SQLite DB to Azure SQL Server |
| `--check-admin-count` | Prints the number of admin accounts to the console |
| `--reset-app-data` | Resets app data (preserves staff accounts) |

Example:

```bash
dotnet run -- --check-admin-count
```

---

## Deployment

Deployment targets Azure App Service. The ARM templates in `infra/azure/` provision all required Azure resources.

### Azure Resources (ARM Template)

- Azure SQL logical server (`onejaxsqlserver`)
- Azure SQL database (`StrategicDashboardDB`)
- Linux App Service Plan
- Linux Web App (`OneJaxStrategicDashboard`)
- User-assigned managed identity with GitHub OIDC federation
- SQL firewall rules

### Deployment Flow

1. Deploy Azure resources using the ARM template in `infra/azure/onejax.template.json`
2. Add required GitHub Actions secrets (connection strings, credentials)
3. Let GitHub Actions run EF Core migrations against Azure SQL
4. Deploy the published app to `OneJaxStrategicDashboard`
5. Verify the app connects to Azure SQL on startup

> See [infra/azure/README.md](infra/azure/README.md) for detailed infrastructure documentation.

### Security Notes

- Never commit `sqlAdministratorPassword` or real connection strings to source control
- Use GitHub Actions secrets or Azure Key Vault for credentials
- The example parameters file at `infra/azure/onejax.example.parameters.json` is safe to commit; `onejax.parameters.json` (with real values) is gitignored

---

## Project Structure

```
OneJax-Dashboard/
├── StrategicDashboard/          # Main ASP.NET Core project
│   ├── Controllers/             # MVC controllers (metrics, events, staff, etc.)
│   ├── Models/                  # Entity and view models
│   ├── Views/                   # Razor views
│   ├── Services/                # Business logic services
│   ├── Database/                # DbContext, migrations, configuration
│   ├── Migrations/              # EF Core migration files
│   ├── App_Data/                # JSON data files (e.g., dashboard notes)
│   ├── wwwroot/                 # Static assets (CSS, JS, images)
│   ├── Program.cs               # App entry point and startup config
│   ├── appsettings.json         # Base configuration
│   ├── appsettings.Development.json
│   └── appsettings.Production.json
├── Services/
│   └── MetricsService.cs        # Shared metrics service
├── infra/
│   └── azure/                   # ARM templates and deployment docs
└── README.md
```
