using FixTrailSecurity.Core.Entities;
using Microsoft.EntityFrameworkCore;
namespace FixTrailSecurity.Core.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<Finding> Findings { get; set; }
        public DbSet<FindingOccurrence> FindingOccurrences { get; set; }
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
            modelBuilder.Entity<Finding>(entity => 
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(300);
                entity.Property(e => e.Source).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SourceSeverity).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ExternalId).HasMaxLength(200);
                // Relation to Asset
                entity.HasOne<Asset>()
                      .WithMany()
                      .HasForeignKey(e => e.AssetId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<FindingOccurrence>(entity =>
            {
                entity.HasKey(e => e.Id);
                // Relation to Finding
                entity.HasOne<Finding>()
                      .WithMany()
                      .HasForeignKey(e => e.FindingId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
