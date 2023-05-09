using Microsoft.EntityFrameworkCore;
using Paczki.Models;
//using System.Data.Entity;

namespace Paczki
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<Package> Packages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Package>().HasMany(p => p.Deliveries)
                .WithOne(d => d.Package)
                .HasForeignKey(d => d.PackageRefId)
                .IsRequired();
        }
    }
}
