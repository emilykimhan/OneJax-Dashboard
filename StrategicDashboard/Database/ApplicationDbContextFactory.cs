using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace OneJaxDashboard.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var basePath = ResolveBasePath();
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                ?? Environments.Development;

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var databaseSettings = DatabaseConfiguration.Resolve(configuration, environmentName);

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            DatabaseConfiguration.Configure(optionsBuilder, databaseSettings);

            return new ApplicationDbContext(optionsBuilder.Options);
        }

        private static string ResolveBasePath()
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            if (File.Exists(Path.Combine(currentDirectory, "appsettings.json")))
            {
                return currentDirectory;
            }

            var projectDirectory = Path.Combine(currentDirectory, "StrategicDashboard");
            if (File.Exists(Path.Combine(projectDirectory, "appsettings.json")))
            {
                return projectDirectory;
            }

            return currentDirectory;
        }
    }
}
