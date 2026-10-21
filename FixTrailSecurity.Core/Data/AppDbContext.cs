using FixTrailSecurity.Core.Entities;
using Microsoft.EntityFrameworkCore;
namespace FixTrailSecurity.Core.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Asset> Assets { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Asset>(entity => 
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Environment).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Owner).IsRequired().HasMaxLength(100);
            });
        }
    }
}
