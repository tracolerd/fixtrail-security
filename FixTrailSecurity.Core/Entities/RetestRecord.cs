using System;
namespace FixTrailSecurity.Core.Entities
{
    public class RetestRecord
    {
        public Guid Id { get; private set; }
        public Guid FindingId { get; private set; }
        public bool IsSuccessful { get; private set; }
        public string? Notes { get; private set; }
        public string RetestedBy { get; private set; }
        public DateTime RetestedAt { get; private set; }
        protected RetestRecord() { RetestedBy = null!; }
        public RetestRecord(Guid findingId, bool isSuccessful, string retestedBy, string? notes = null)
        {
            if (findingId == Guid.Empty) throw new ArgumentException("FindingId is required.");
            if (string.IsNullOrWhiteSpace(retestedBy)) throw new ArgumentException("RetestedBy is required.");
            Id = Guid.NewGuid();
            FindingId = findingId;
            IsSuccessful = isSuccessful;
            RetestedBy = retestedBy;
            Notes = notes;
            RetestedAt = DateTime.UtcNow;
        }
    }
}
