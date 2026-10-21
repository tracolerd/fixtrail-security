using System;
using FixTrailSecurity.Core.Entities;
using Xunit;
namespace FixTrailSecurity.Tests
{
    public class AssetTests
    {
        [Fact]
        public void CreateAsset_WithValidData_ShouldSetProperties()
        {
            // Arrange
            var name = "Web Server";
            var env = "Production";
            var owner = "Security Team";
            // Act
            var asset = new Asset(name, env, owner);
            // Assert
            Assert.Equal(name, asset.Name);
            Assert.Equal(env, asset.Environment);
            Assert.Equal(owner, asset.Owner);
            Assert.NotEqual(Guid.Empty, asset.Id);
        }
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void CreateAsset_WithEmptyName_ShouldThrowException(string? invalidName)
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => new Asset(invalidName!, "Env", "Owner"));
            Assert.Equal("Asset name is required.", ex.Message);
        }
    }
}
