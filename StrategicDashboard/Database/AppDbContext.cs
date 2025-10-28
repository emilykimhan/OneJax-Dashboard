using Microsoft.EntityFrameworkCore;
using OneJaxDashboard.Models;

namespace OneJaxDashboard.Data
{
    public class ApplicationDbContext : DbContext
    {
        
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<StaffSurvey_22D> StaffSurveys_22D { get; set; } = default!;
        public DbSet<ProfessionalDevelopment> ProfessionalDevelopments { get; set; } = default!;
        
      
    }
}