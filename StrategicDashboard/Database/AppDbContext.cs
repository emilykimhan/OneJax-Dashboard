using Microsoft.EntityFrameworkCore;
using StrategicDashboard.Models;
using OneJaxDashboard.Models;

namespace OneJaxDashboard.Data
{
    public class ApplicationDbContext : DbContext
    {
        
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Staff> StaffMembers { get; set; }
        public DbSet<StaffSurvey_22D> StaffSurveys_22D { get; set; } = default!;
        public DbSet<ProfessionalDevelopment> ProfessionalDevelopments { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique username for staff
            modelBuilder.Entity<Staff>()
                .HasIndex(s => s.Username)
                .IsUnique();
        }
    }
}