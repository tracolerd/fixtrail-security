using System.Text;

namespace FixTrailSecurity.Core.Security;

/// <summary>Validates JWT signing secrets before authentication is configured.</summary>
public static class JwtSecretValidator
{
    private static readonly string[] PlaceholderMarkers =
    [
        "YOUR_",
        "CHANGE_ME",
        "REPLACE_ME",
        "PLEASE_CHANGE",
        "EXAMPLE_SECRET"
    ];

    public static string RequireStrongSecret(string? secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException(
                "JWT Secret is missing. Configure JwtSettings:Secret using a secret store.");
        }

        if (Encoding.UTF8.GetByteCount(secret) < 32)
        {
            throw new InvalidOperationException(
                "JWT Secret must contain at least 32 UTF-8 bytes.");
        }

        if (PlaceholderMarkers.Any(marker =>
                secret.Contains(marker, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                "JWT Secret appears to contain a placeholder value. Configure a unique secret.");
        }

        return secret;
    }
}
