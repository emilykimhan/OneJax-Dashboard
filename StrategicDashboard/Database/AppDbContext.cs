using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Models;
using OneJax.StrategicDashboard.Models;

namespace OneJaxDashboard.Data
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Your friend's existing tables
        public DbSet<StaffSurvey_22D> StaffSurveys_22D { get; set; } = default!;
        public DbSet<ProfessionalDevelopment> ProfessionalDevelopments { get; set; } = default!;

        // Dashboard tables
        public DbSet<StrategicGoal> StrategicGoals { get; set; } = default!;
        public DbSet<GoalMetric> GoalMetrics { get; set; } = default!;
        public DbSet<Event> Events { get; set; } = default!;
        
        // Core Strategies 
        public DbSet<Strategy> Strategies { get; set; }
      
    }
}