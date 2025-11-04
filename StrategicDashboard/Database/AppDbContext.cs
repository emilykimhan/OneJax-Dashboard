using Microsoft.EntityFrameworkCore;
using StrategicDashboard.Models;

namespace OneJaxDashboard.Data
{
    public class ApplicationDbContext : DbContext
    {
        
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Staff> StaffMembers { get; set; }

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