using Microsoft.EntityFrameworkCore;
using OneJax.StrategicDashboard.Models;
using OneJaxDashboard.Models;
using StrategicDashboard.Models;

namespace StrategicDashboard.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Your friend's existing tables
        // Staff functionality commented out - will use existing tables instead
        // public DbSet<Staff> StaffMembers { get; set; }
        public DbSet<StaffSurvey_22D> StaffSurveys_22D { get; set; } = default!;
        public DbSet<ProfessionalDevelopment> ProfessionalDevelopments { get; set; } = default!;
        
        // New entities for Events and Strategic Planning
        public DbSet<Event> Events { get; set; } = default!;
        public DbSet<StrategicGoal> StrategicGoals { get; set; } = default!;
        public DbSet<Strategy> Strategies { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Add unique index for username on StaffSurvey_22D table
            modelBuilder.Entity<StaffSurvey_22D>()
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