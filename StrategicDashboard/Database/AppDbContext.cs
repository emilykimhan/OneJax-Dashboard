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

        
      
    }
}