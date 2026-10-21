using System;
namespace FixTrailSecurity.Core.Entities
{
    public class FindingOccurrence
    {
        public Guid Id { get; private set; }
        public Guid FindingId { get; private set; }
        public DateTime ObservedAt { get; private set; }
        public string? Notes { get; private set; }
        protected FindingOccurrence() { }
        public FindingOccurrence(Guid findingId, string? notes = null)
        {
            if (findingId == Guid.Empty) throw new ArgumentException("FindingId is required.");
            Id = Guid.NewGuid();
            FindingId = findingId;
            ObservedAt = DateTime.UtcNow;
            Notes = notes;
        }
    }
}
