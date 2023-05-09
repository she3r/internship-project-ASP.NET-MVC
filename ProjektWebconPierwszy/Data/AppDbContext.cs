using Microsoft.EntityFrameworkCore;
using ProjektWebconPierwszy.Models;

namespace ProjektWebconPierwszy.Data
{
    public class AppDbContext : DbContext 
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
        {
            
        }
        public DbSet<Category> Categories { get; set; }
    }
}
