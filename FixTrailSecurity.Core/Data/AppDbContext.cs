using FixTrailSecurity.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace FixTrailSecurity.Core.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
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
            modelBuilder.Entity<Asset>(e => { e.HasKey(x => x.Id); e.Property(x => x.Name).IsRequired().HasMaxLength(200); });
            modelBuilder.Entity<Finding>(e => { 
                e.HasKey(x => x.Id); 
                e.HasOne<Asset>().WithMany().HasForeignKey(x => x.AssetId).OnDelete(DeleteBehavior.Cascade); 
                // Concurrency-safe Deduplication Constraint
                e.HasIndex(x => new { x.AssetId, x.Title, x.Source }).IsUnique(); 
            });
            modelBuilder.Entity<FindingOccurrence>(e => { e.HasKey(x => x.Id); e.HasOne<Finding>().WithMany().HasForeignKey(x => x.FindingId).OnDelete(DeleteBehavior.Cascade); });
            modelBuilder.Entity<RemediationTask>(e => { e.HasKey(x => x.Id); e.HasOne<Finding>().WithMany().HasForeignKey(x => x.FindingId).OnDelete(DeleteBehavior.Cascade); });
            modelBuilder.Entity<EvidenceReference>(e => { e.HasKey(x => x.Id); e.HasOne<RemediationTask>().WithMany().HasForeignKey(x => x.RemediationTaskId).OnDelete(DeleteBehavior.Cascade); });
            modelBuilder.Entity<RetestRecord>(e => { e.HasKey(x => x.Id); e.HasOne<Finding>().WithMany().HasForeignKey(x => x.FindingId).OnDelete(DeleteBehavior.Cascade); });
            modelBuilder.Entity<AuditEvent>(e => { e.HasKey(x => x.Id); e.HasOne<Finding>().WithMany().HasForeignKey(x => x.FindingId).OnDelete(DeleteBehavior.Cascade); });
        }
    }
}
