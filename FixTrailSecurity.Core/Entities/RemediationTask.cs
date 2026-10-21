using System;
namespace FixTrailSecurity.Core.Entities
{
    public class RemediationTask
    {
        public Guid Id { get; private set; }
        public Guid FindingId { get; private set; }
        public string AssignedTo { get; private set; }
        public DateTime DueDate { get; private set; }
        public string? Notes { get; private set; }
        public DateTime CreatedAt { get; private set; }
        protected RemediationTask() { AssignedTo = null!; }
        public RemediationTask(Guid findingId, string assignedTo, DateTime dueDate, string? notes = null)
        {
            if (findingId == Guid.Empty) throw new ArgumentException("FindingId is required.");
            if (string.IsNullOrWhiteSpace(assignedTo)) throw new ArgumentException("AssignedTo is required.");
            Id = Guid.NewGuid();
            FindingId = findingId;
            AssignedTo = assignedTo;
            DueDate = dueDate;
            Notes = notes;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
