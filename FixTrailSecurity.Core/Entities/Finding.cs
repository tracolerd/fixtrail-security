using System;
using FixTrailSecurity.Core.Enums;
namespace FixTrailSecurity.Core.Entities
{
    public class Finding
    {
        public Guid Id { get; private set; }
        public Guid AssetId { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string Source { get; private set; }
        public string? ExternalId { get; private set; }
        public string SourceSeverity { get; private set; }
        public FindingStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        protected Finding() 
        { 
            Title = null!; Description = null!; Source = null!; SourceSeverity = null!; 
        }
        public Finding(Guid assetId, string title, string description, string source, string sourceSeverity, string? externalId = null)
        {
            if (assetId == Guid.Empty) throw new ArgumentException("AssetId is required.");
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.");
            Id = Guid.NewGuid();
            AssetId = assetId;
            Title = title;
            Description = description ?? string.Empty;
            Source = source;
            SourceSeverity = sourceSeverity;
            ExternalId = externalId;
            Status = FindingStatus.Open;
            CreatedAt = DateTime.UtcNow;
        }
        public bool CanTransitionTo(FindingStatus newStatus)
        {
            return (Status, newStatus) switch
            {
                (FindingStatus.Open, FindingStatus.Assigned) => true,
                (FindingStatus.Assigned, FindingStatus.Mitigated) => true,
                (FindingStatus.Reopened, FindingStatus.Assigned) => true,
                (FindingStatus.Reopened, FindingStatus.Mitigated) => true,
                (FindingStatus.Mitigated, FindingStatus.Closed) => true,
                (FindingStatus.Mitigated, FindingStatus.Reopened) => true,
                (FindingStatus.Closed, FindingStatus.Reopened) => true,
                _ => false
            };
        }

        public bool TryTransitionTo(FindingStatus newStatus)
        {
            if (!CanTransitionTo(newStatus))
                return false;

            Status = newStatus;
            return true;
        }

        // Kept as a guarded domain API for existing callers; invalid transitions
        // now fail explicitly instead of silently overwriting workflow state.
        public void UpdateStatus(FindingStatus newStatus)
        {
            if (!TryTransitionTo(newStatus))
                throw new InvalidOperationException($"Invalid finding status transition: {Status} -> {newStatus}.");
        }
    }
}
