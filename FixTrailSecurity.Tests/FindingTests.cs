using System;
using FixTrailSecurity.Core.Entities;
using FixTrailSecurity.Core.Enums;
using Xunit;
namespace FixTrailSecurity.Tests
{
    public class FindingTests
    {
        [Fact]
        public void CreateFinding_WithValidData_ShouldInitializeCorrectly()
        {
            // Arrange
            var assetId = Guid.NewGuid();
            var title = "SQL Injection";
            var desc = "Found in login parameter";
            var source = "OWASP ZAP";
            var severity = "High";
            // Act
            var finding = new Finding(assetId, title, desc, source, severity);
            // Assert
            Assert.NotEqual(Guid.Empty, finding.Id);
            Assert.Equal(FindingStatus.Open, finding.Status);
            Assert.Equal(title, finding.Title);
        }
        [Fact]
        public void CreateFinding_WithEmptyTitle_ShouldThrowException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => new Finding(Guid.NewGuid(), "", "Desc", "Nmap", "Low"));
            Assert.Equal("Title is required.", ex.Message);
        }
        [Fact]
        public void CanTransitionTo_RejectsIllegalStatusJumps()
        {
            var finding = new Finding(Guid.NewGuid(), "XSS", "Desc", "Burp", "Medium");

            Assert.False(finding.CanTransitionTo(FindingStatus.Closed));
            Assert.False(finding.CanTransitionTo(FindingStatus.Mitigated));
            Assert.True(finding.CanTransitionTo(FindingStatus.Assigned));

            finding.UpdateStatus(FindingStatus.Assigned);
            Assert.False(finding.CanTransitionTo(FindingStatus.Closed));
            Assert.True(finding.CanTransitionTo(FindingStatus.Mitigated));
        }

        [Fact]
        public void TryTransitionTo_RejectsInvalidTransitionWithoutChangingState()
        {
            var finding = new Finding(Guid.NewGuid(), "XSS", "Desc", "Burp", "Medium");

            Assert.False(finding.TryTransitionTo(FindingStatus.Closed));
            Assert.Equal(FindingStatus.Open, finding.Status);
            Assert.Throws<InvalidOperationException>(() => finding.UpdateStatus(FindingStatus.Closed));
        }

        [Fact]
        public void UpdateStatus_ShouldChangeFindingStatus()
        {
            // Arrange
            var finding = new Finding(Guid.NewGuid(), "XSS", "Desc", "Burp", "Medium");
            // Act
            finding.UpdateStatus(FindingStatus.Assigned);
            // Assert
            Assert.Equal(FindingStatus.Assigned, finding.Status);
        }
    }
}
