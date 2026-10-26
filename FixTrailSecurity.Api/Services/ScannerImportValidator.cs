using FixTrailSecurity.Api.DTOs;

namespace FixTrailSecurity.Api.Services;

public static class ScannerImportValidator
{
    public const int MaxFindings = 10_000;
    public const int MaxTitleLength = 512;
    public const int MaxDescriptionLength = 10_000;
    public const int MaxSourceLength = 128;
    public const int MaxSeverityLength = 64;
    public const int MaxExternalIdLength = 256;

    /// <summary>
    /// Validate the entire batch before any database mutation. A non-null return
    /// value is a safe, actionable error message; it never includes imported data.
    /// </summary>
    public static string? Validate(IReadOnlyList<ImportedFindingDto?>? findings)
    {
        if (findings is null || findings.Count == 0)
            return "The import must contain at least one finding.";

        if (findings.Count > MaxFindings)
            return $"The import exceeds the maximum of {MaxFindings} findings.";

        for (var index = 0; index < findings.Count; index++)
        {
            var item = findings[index];
            if (item is null)
                return $"Finding at index {index} is null.";

            if (string.IsNullOrWhiteSpace(item.Title))
                return $"Finding at index {index} is missing a title.";
            if (item.Title.Length > MaxTitleLength)
                return $"Finding at index {index} exceeds the title length limit.";

            if (string.IsNullOrWhiteSpace(item.Source))
                return $"Finding at index {index} is missing a source.";
            if (item.Source.Length > MaxSourceLength)
                return $"Finding at index {index} exceeds the source length limit.";

            if (string.IsNullOrWhiteSpace(item.SourceSeverity))
                return $"Finding at index {index} is missing a severity.";
            if (item.SourceSeverity.Length > MaxSeverityLength)
                return $"Finding at index {index} exceeds the severity length limit.";

            if (item.Description is { Length: > MaxDescriptionLength })
                return $"Finding at index {index} exceeds the description length limit.";

            if (item.ExternalId is { Length: > MaxExternalIdLength })
                return $"Finding at index {index} exceeds the external ID length limit.";
        }

        return null;
    }
}
