using System;
namespace FixTrailSecurity.Core.Entities
{
    public class AuditEvent
    {
        public Guid Id { get; private set; }
        public Guid FindingId { get; private set; }
        public string Action { get; private set; }
        public string Details { get; private set; }
        public string PerformedBy { get; private set; }
        public DateTime Timestamp { get; private set; }
        protected AuditEvent() { Action = null!; Details = null!; PerformedBy = null!; }
        public AuditEvent(Guid findingId, string action, string details, string performedBy)
        {
            Id = Guid.NewGuid();
            FindingId = findingId;
            Action = action;
            Details = details;
            PerformedBy = performedBy;
            Timestamp = DateTime.UtcNow;
        }
    }
}
