# Azure Infrastructure

This folder stores Azure infrastructure deployment files for OneJax.

Files:
- `onejax.template.json`: ARM template for Azure resources
- `onejax.example.parameters.json`: safe example parameters file

The App Service name in these files is aligned to:
- `OneJaxStrategicDashboard`

Do not commit real secrets to this repo.

In particular, keep `sqlAdministratorPassword` out of source control.
Use one of these instead:
- Azure CLI `--parameters sqlAdministratorPassword=<value>`
- GitHub Actions secrets
- Azure Key Vault

If you want a real local parameters file, create one such as:
- `infra/azure/onejax.parameters.json`

That file is ignored by Git.

## What The ARM Template Already Covers

`onejax.template.json` currently provisions:
- an Azure SQL logical server named `onejaxsqlserver`
- an Azure SQL database named `StrategicDashboardDB`
- a Linux App Service plan
- a Linux Web App named `OneJaxStrategicDashboard`
- a user-assigned managed identity with GitHub OIDC federation
- SQL firewall rules, including `AllowAllWindowsAzureIps`

What it does not fully configure for the app:
- the production Azure SQL connection string for the web app
- GitHub repository secrets
- EF Core migration execution by itself

## Recommended Deployment Flow

Use the deployment flow in this order:
1. Deploy or confirm the Azure resources from the ARM template.
2. Add the GitHub secrets used by the workflow.
3. Let GitHub Actions run EF Core migrations against Azure SQL.
4. Deploy the published app to `OneJaxStrategicDashboard`.
5. Let the app start in Production and verify it can connect to Azure SQL.

The app already auto-applies pending EF Core migrations on startup in [Program.cs](/Users/emily/OneJax/OneJax-Dashboard/StrategicDashboard/Program.cs:102), so once the production connection string is set correctly the web app can also finish pending schema updates during startup.

## GitHub Secrets To Add

The workflow expects these GitHub secrets:
- `AZUREAPPSERVICE_CLIENTID_247393796505474D9511451E28AA201A`
- `AZUREAPPSERVICE_TENANTID_5FA48E0ABCED43B19127DB9D18E39BCA`
- `AZUREAPPSERVICE_SUBSCRIPTIONID_A392EF3D1FED4DC89A11006C68A9DA76`
- `AZURE_SQL_CONNECTION_STRING`

Example value for `AZURE_SQL_CONNECTION_STRING`:

```text
Server=tcp:onejaxsqlserver.database.windows.net,1433;Initial Catalog=StrategicDashboardDB;Persist Security Info=False;User ID=<sql-admin-user>;Password=<sql-admin-password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

## Deploying The ARM Template

You can deploy the ARM template from Azure Cloud Shell or a local Azure CLI session:

```bash
az deployment group create \
  --resource-group <resource-group-name> \
  --template-file infra/azure/onejax.template.json \
  --parameters @infra/azure/onejax.parameters.json \
  --parameters sqlAdministratorPassword='<strong-password>'
```

## Running Database Migrations

There are three practical ways to run the database migrations.

### Option 1: GitHub Actions

The workflow at [.github/workflows/main_onejaxstrategicdashboard.yml](/Users/emily/OneJax/OneJax-Dashboard/.github/workflows/main_onejaxstrategicdashboard.yml:1) now:
- builds and publishes the app
- runs `dotnet ef database update` against Azure SQL
- pushes the production app settings into the App Service
- deploys the site to `OneJaxStrategicDashboard`

This is the cleanest default path for regular deployments.

### Option 2: App Startup In Azure

The app calls `db.Database.Migrate()` during startup in [Program.cs](/Users/emily/OneJax/OneJax-Dashboard/StrategicDashboard/Program.cs:102).

That means you can:
1. Open the Azure portal.
2. Go to the `OneJaxStrategicDashboard` App Service.
3. Open `Settings` -> `Environment variables`.
4. Add:
   - `ASPNETCORE_ENVIRONMENT=Production`
   - `DatabaseProvider=SqlServer`
   - `ConnectionStrings__AzureSqlConnection=<your Azure SQL connection string>`
5. Save the settings and restart the web app.

When the site restarts, the app should connect to Azure SQL and apply pending EF migrations automatically.

Important note:
- the Azure SQL portal itself does not run `dotnet ef` migrations directly
- the portal can run raw SQL queries, but EF migrations are normally run by the app, by the CLI, or by GitHub Actions

### Option 3: Run EF Migrations Manually

From your machine or Azure Cloud Shell:

```bash
ASPNETCORE_ENVIRONMENT=Production \
DatabaseProvider=SqlServer \
ConnectionStrings__AzureSqlConnection="Server=tcp:onejaxsqlserver.database.windows.net,1433;Initial Catalog=StrategicDashboardDB;Persist Security Info=False;User ID=<sql-admin-user>;Password=<sql-admin-password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" \
dotnet ef database update \
  --project StrategicDashboard/StrategicDashboard.csproj \
  --startup-project StrategicDashboard/StrategicDashboard.csproj
```

## Copying Existing SQLite Data Into Azure SQL

This repo also has a built-in SQLite-to-SQL Server data migration mode in [Program.cs](/Users/emily/OneJax/OneJax-Dashboard/StrategicDashboard/Program.cs:18) and [SqliteToSqlServerMigrator.cs](/Users/emily/OneJax/OneJax-Dashboard/StrategicDashboard/Database/SqliteToSqlServerMigrator.cs:1).

Use it if you need both:
- the Azure SQL schema
- the existing local SQLite data copied into Azure SQL

Run it like this:

```bash
ASPNETCORE_ENVIRONMENT=Production \
DatabaseProvider=SqlServer \
ConnectionStrings__AzureSqlConnection="Server=tcp:onejaxsqlserver.database.windows.net,1433;Initial Catalog=StrategicDashboardDB;Persist Security Info=False;User ID=<sql-admin-user>;Password=<sql-admin-password>;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;" \
ConnectionStrings__SqliteMigrationSource="Data Source=StrategicDashboardDB.db" \
dotnet run --project StrategicDashboard/StrategicDashboard.csproj -- --migrate-sqlite-to-sqlserver
```

Important:
- the target Azure SQL database must be empty for this data copy to work
- this is for one-time data migration, not for normal recurring deployments

## Quick Portal Checklist

If you want the shortest version for the portal:
1. Confirm the SQL server and database exist from the ARM template.
2. Confirm the App Service exists.
3. Set the App Service environment variables for `Production`, `SqlServer`, and `ConnectionStrings__AzureSqlConnection`.
4. Push to `main` so GitHub Actions runs the migration and deployment workflow.
5. Open the site and check App Service logs if startup fails.
