using System;
using FixTrailSecurity.Core.Entities;
using FixTrailSecurity.Core.Enums;
using Xunit;
namespace FixTrailSecurity.Tests
{
    public class WorkflowTests
    {
        [Fact]
        public void AssignTask_ShouldCreateTaskAndAllowEvidenceSubmission()
        {
            // Arrange
            var findingId = Guid.NewGuid();
            var task = new RemediationTask(findingId, "DevTeam", DateTime.UtcNow.AddDays(7));
            // Act
            var evidence = new EvidenceReference(task.Id, "https://github.com/pr/123", "Fixed XSS");
            // Assert
            Assert.Equal(task.Id, evidence.RemediationTaskId);
            Assert.Equal("https://github.com/pr/123", evidence.EvidenceUrlOrPath);
        }
        [Fact]
        public void Retest_ShouldRecordSuccessCorrectly()
        {
            // Arrange
            var findingId = Guid.NewGuid();
            // Act
            var retest = new RetestRecord(findingId, true, "SecurityAnalyst", "Verified fix");
            // Assert
            Assert.True(retest.IsSuccessful);
            Assert.Equal("SecurityAnalyst", retest.RetestedBy);
        }
        [Fact]
        public void AuditEvent_ShouldInitializeCorrectly()
        {
            // Arrange
            var findingId = Guid.NewGuid();
            // Act
            var audit = new AuditEvent(findingId, "StatusChange", "Moved to Closed", "System");
            // Assert
            Assert.Equal("StatusChange", audit.Action);
            Assert.Equal("Moved to Closed", audit.Details);
            Assert.NotEqual(default, audit.Timestamp);
        }
    }
}
