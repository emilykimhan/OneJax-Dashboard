using Microsoft.EntityFrameworkCore;
using OneJax.StrategicDashboard.Models;

namespace StrategicDashboard.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Add DbSets for your models
        public DbSet<Strategy> Strategies { get; set; }
        public DbSet<StrategicGoal> StrategicGoals { get; set; }
    }
}