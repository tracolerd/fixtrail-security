using System;
namespace FixTrailSecurity.Core.Entities
{
    public class EvidenceReference
    {
        public Guid Id { get; private set; }
        public Guid RemediationTaskId { get; private set; }
        public string EvidenceUrlOrPath { get; private set; }
        public string? Description { get; private set; }
        public DateTime UploadedAt { get; private set; }
        protected EvidenceReference() { EvidenceUrlOrPath = null!; }
        public EvidenceReference(Guid remediationTaskId, string evidenceUrlOrPath, string? description = null)
        {
            if (remediationTaskId == Guid.Empty) throw new ArgumentException("RemediationTaskId is required.");
            if (string.IsNullOrWhiteSpace(evidenceUrlOrPath)) throw new ArgumentException("Evidence URL/Path is required.");
            Id = Guid.NewGuid();
            RemediationTaskId = remediationTaskId;
            EvidenceUrlOrPath = evidenceUrlOrPath;
            Description = description;
            UploadedAt = DateTime.UtcNow;
        }
    }
}
