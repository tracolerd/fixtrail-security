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
        public DbSet<RemediationTask> RemediationTasks { get; set; }
        public DbSet<EvidenceReference> EvidenceReferences { get; set; }
        public DbSet<RetestRecord> RetestRecords { get; set; }
        public DbSet<AuditEvent> AuditEvents { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Asset Configuration
            modelBuilder.Entity<Asset>(entity => 
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Environment).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Owner).IsRequired().HasMaxLength(100);
            });
            // Finding Configuration
            modelBuilder.Entity<Finding>(entity => 
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(300);
                entity.Property(e => e.Source).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SourceSeverity).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ExternalId).HasMaxLength(200);
                entity.HasOne<Asset>().WithMany().HasForeignKey(e => e.AssetId).OnDelete(DeleteBehavior.Cascade);
            });
            // Occurrence Configuration
            modelBuilder.Entity<FindingOccurrence>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne<Finding>().WithMany().HasForeignKey(e => e.FindingId).OnDelete(DeleteBehavior.Cascade);
            });
            // RemediationTask Configuration
            modelBuilder.Entity<RemediationTask>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AssignedTo).IsRequired().HasMaxLength(100);
                entity.HasOne<Finding>().WithMany().HasForeignKey(e => e.FindingId).OnDelete(DeleteBehavior.Cascade);
            });
            // EvidenceReference Configuration
            modelBuilder.Entity<EvidenceReference>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EvidenceUrlOrPath).IsRequired().HasMaxLength(500);
                entity.HasOne<RemediationTask>().WithMany().HasForeignKey(e => e.RemediationTaskId).OnDelete(DeleteBehavior.Cascade);
            });
            // RetestRecord Configuration
            modelBuilder.Entity<RetestRecord>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RetestedBy).IsRequired().HasMaxLength(100);
                entity.HasOne<Finding>().WithMany().HasForeignKey(e => e.FindingId).OnDelete(DeleteBehavior.Cascade);
            });
            // AuditEvent Configuration
            modelBuilder.Entity<AuditEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PerformedBy).IsRequired().HasMaxLength(100);
                entity.HasOne<Finding>().WithMany().HasForeignKey(e => e.FindingId).OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
