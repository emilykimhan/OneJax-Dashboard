using Microsoft.EntityFrameworkCore;
using VMS.Models;

namespace VMS.Data
{
    public class ApplicationDbContext : DbContext
    {
        // ✅ EF needs this constructor for dependency injection
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ✅ Add your DbSet here — this becomes your database table
        public DbSet<ThreeDDataEntry> MediaPlacements { get; set; }
    }
}