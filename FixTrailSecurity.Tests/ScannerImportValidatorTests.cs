using FixTrailSecurity.Api.DTOs;
using FixTrailSecurity.Api.Services;
using Xunit;

namespace FixTrailSecurity.Tests;

public class ScannerImportValidatorTests
{
    private static ImportedFindingDto ValidFinding() => new()
    {
        Title = "Open Port: 443/tcp",
        Description = "A test finding.",
        Source = "Nmap",
        SourceSeverity = "Info",
        ExternalId = "nmap-443"
    };

    [Fact]
    public void Validate_AcceptsValidFinding()
    {
        Assert.Null(ScannerImportValidator.Validate(new ImportedFindingDto?[] { ValidFinding() }));
    }

    [Fact]
    public void Validate_RejectsNullRowsAndEmptyBatches()
    {
        Assert.NotNull(ScannerImportValidator.Validate(Array.Empty<ImportedFindingDto?>()));
        Assert.Contains("is null", ScannerImportValidator.Validate(new ImportedFindingDto?[] { null })!);
    }

    [Fact]
    public void Validate_RejectsRequiredFieldsThatAreBlank()
    {
        var finding = ValidFinding();
        finding.Title = "  ";
        Assert.Contains("missing a title", ScannerImportValidator.Validate(new ImportedFindingDto?[] { finding })!);
    }

    [Fact]
    public void Validate_RejectsOversizedFields()
    {
        var finding = ValidFinding();
        finding.Description = new string('x', ScannerImportValidator.MaxDescriptionLength + 1);
        Assert.Contains("description length", ScannerImportValidator.Validate(new ImportedFindingDto?[] { finding })!);
    }

    [Fact]
    public void Validate_RejectsOversizedBatches()
    {
        var rows = Enumerable.Range(0, ScannerImportValidator.MaxFindings + 1)
            .Select(_ => (ImportedFindingDto?)ValidFinding())
            .ToArray();
        Assert.Contains("maximum", ScannerImportValidator.Validate(rows)!);
    }
}
