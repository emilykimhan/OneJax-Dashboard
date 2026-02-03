using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Models;
using OneJaxDashboard.Data;

namespace OneJaxDashboard.Data
{
    public class ApplicationDbContext : DbContext
    {
        // migrations for everything, then update the database 
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Data entry tables
        public DbSet<StaffSurvey_22D> StaffSurveys_22D { get; set; } = default!;
        public DbSet<ProfessionalDevelopment> ProfessionalDevelopments { get; set; } = default!;
        public DbSet<MediaPlacements_3D> MediaPlacements_3D { get; set; } = default!;
        public DbSet<WebsiteTraffic_4D> WebsiteTraffic { get; set; } = default!;
        public DbSet<Comm_rate20D> CommunicationRate { get; set; } = default!;
        

        // Dashboard tables
        public DbSet<StrategicGoal> StrategicGoals { get; set; } = default!;
        public DbSet<GoalMetric> GoalMetrics { get; set; } = default!;
        public DbSet<Event> Events { get; set; } = default!;

        // Core Strategies 
        public DbSet<Strategy> Strategies { get; set; }

        //Account tables
        
        
        public DbSet<Staffauth> Staffauth { get; set; } = default!;
           protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Add unique index for username on Staffauth table
            modelBuilder.Entity<Staffauth>()
                .HasIndex(s => s.Username)
                .IsUnique()
                .HasFilter("[Username] IS NOT NULL"); // Only enforce uniqueness for non-null usernames

            // Configure Event relationships
            modelBuilder.Entity<Event>()
                .HasOne(e => e.StrategicGoal)
                .WithMany()
                .HasForeignKey(e => e.StrategicGoalId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Strategy)
                .WithMany()
                .HasForeignKey(e => e.StrategyId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Strategy relationships
            modelBuilder.Entity<Strategy>()
                .HasOne(s => s.StrategicGoal)
                .WithMany(g => g.Strategies)
                .HasForeignKey(s => s.StrategicGoalId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
