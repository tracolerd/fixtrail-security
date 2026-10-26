using System.Text.Json;
using FixTrailSecurity.Api.DTOs;
using FixTrailSecurity.Api.Services;
using FixTrailSecurity.Core.Data;
using FixTrailSecurity.Core.Entities;
using FixTrailSecurity.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FixTrailSecurity.Api.Controllers
{
    [Authorize(Policy = "RequireAnalystOrAdmin")]
    [ApiController]
    [Route("api/[controller]")]
    public class ImporterController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly NmapXmlParserService _nmapParser;
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB Limit
        public ImporterController(AppDbContext context, NmapXmlParserService nmapParser)
        {
            _context = context;
            _nmapParser = nmapParser;
        }
        [HttpPost("upload/{assetId}")]
        public async Task<IActionResult> UploadFindings(Guid assetId, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest(new { message = "File is empty or missing." });
            if (file.Length > MaxFileSize) return BadRequest(new { message = "File exceeds the 5MB size limit." });
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".json" && ext != ".xml") return BadRequest(new { message = "Only JSON and XML files are supported." });
            var assetExists = await _context.Assets.AnyAsync(a => a.Id == assetId);
            if (!assetExists) return NotFound(new { message = "Asset not found." });
            List<ImportedFindingDto?> importedFindings = new();
            try
            {
                using var stream = file.OpenReadStream();
                if (ext == ".json")
                {
                    var jsonOptions = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        MaxDepth = 32
                    };
                    var jsonFindings = await JsonSerializer.DeserializeAsync<List<ImportedFindingDto?>>(stream, jsonOptions);
                    if (jsonFindings != null) importedFindings.AddRange(jsonFindings);
                }
                else if (ext == ".xml")
                {
                    var parsedNmap = _nmapParser.ParseNmapXml(stream);
                    importedFindings.AddRange(parsedNmap.Select(p => new ImportedFindingDto { Title = p.Title, Description = p.Description, Source = p.Source, SourceSeverity = p.SourceSeverity }));
                }
            }
            catch (JsonException)
            {
                return BadRequest(new { message = "The import file contains invalid JSON." });
            }
            catch (InvalidOperationException)
            {
                return BadRequest(new { message = "The import file is invalid or exceeds parser limits." });
            }
            catch (System.Xml.XmlException)
            {
                return BadRequest(new { message = "The import file contains invalid XML." });
            }
            catch (Exception)
            {
                // Do not return parser exception messages, file paths, or internal details.
                return BadRequest(new { message = "Failed to parse the import file." });
            }

            var validationError = ScannerImportValidator.Validate(importedFindings);
            if (validationError != null)
                return BadRequest(new { message = validationError });

            // Validation has proved every item exists and satisfies field limits.
            // Reject the complete batch before opening a transaction if any row is invalid.
            var newFindingsCount = 0;
            var duplicateCount = 0;
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var maybeDto in importedFindings)
                {
                    var dto = maybeDto!;
                    var existingFinding = await _context.Findings.FirstOrDefaultAsync(f => f.AssetId == assetId && f.Title == dto.Title && f.Source == dto.Source);
                    if (existingFinding != null)
                    {
                        _context.FindingOccurrences.Add(new FindingOccurrence(existingFinding.Id, "Imported via batch scanner report."));
                        duplicateCount++;
                    }
                    else
                    {
                        var finding = new Finding(assetId, dto.Title, dto.Description ?? string.Empty, dto.Source, dto.SourceSeverity, dto.ExternalId);
                        _context.Findings.Add(finding);
                        _context.FindingOccurrences.Add(new FindingOccurrence(finding.Id, "Initial import creation."));
                        newFindingsCount++;
                    }
                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { message = "Import completed successfully (All-or-Nothing policy enforced).", processedFindings = importedFindings.Count, newFindingsAdded = newFindingsCount, existingFindingsUpdated = duplicateCount });
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                return Conflict(new { message = "Concurrent modification violation detected. Import rolled back to maintain data integrity." });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "An internal database error occurred. Transaction rolled back safely." });
            }
        }
    }
}
