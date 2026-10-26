using FixTrailSecurity.Core.Security;

namespace FixTrailSecurity.Tests;

public class JwtSecretValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("too-short")]
    [InlineData("YOUR_DB_PASSWORD_HERE_change-me-123456789012")]
    public void RequireStrongSecret_RejectsMissingWeakOrPlaceholderSecrets(string? secret)
    {
        Assert.Throws<InvalidOperationException>(
            () => JwtSecretValidator.RequireStrongSecret(secret));
    }

    [Fact]
    public void RequireStrongSecret_AcceptsClearlyFakeTestFixture()
    {
        const string secret = "unit-test-public-fixture-not-secret-1234567890";

        Assert.Equal(secret, JwtSecretValidator.RequireStrongSecret(secret));
    }
}
