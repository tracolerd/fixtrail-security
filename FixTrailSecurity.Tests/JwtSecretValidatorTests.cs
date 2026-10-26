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
    public void RequireStrongSecret_AcceptsNonPlaceholderSecretAtLeast32Bytes()
    {
        const string secret = "v8K2p9Q4x7N1m6R3s5T0u8W2y4A7c9D1";

        Assert.Equal(secret, JwtSecretValidator.RequireStrongSecret(secret));
    }
}
