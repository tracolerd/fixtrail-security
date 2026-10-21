using System.Text.Json;
using FixTrailSecurity.Api.DTOs;
using FixTrailSecurity.Core.Data;
using FixTrailSecurity.Core.Entities;
using FixTrailSecurity.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FixTrailSecurity.Api.Controllers
{
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
            var ext = Path.GetExtension(file.FileName).ToLower();
            if (ext != ".json" && ext != ".xml")
                return BadRequest(new { message = "Only JSON and XML files are supported." });
            var assetExists = await _context.Assets.AnyAsync(a => a.Id == assetId);
            if (!assetExists) return NotFound(new { message = "Asset not found." });
            List<ImportedFindingDto> importedFindings = new();
            try
            {
                using var stream = file.OpenReadStream();
                if (ext == ".json")
                {
                    var jsonFindings = await JsonSerializer.DeserializeAsync<List<ImportedFindingDto>>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (jsonFindings != null) importedFindings.AddRange(jsonFindings);
                }
                else if (ext == ".xml")
                {
                    var parsedNmap = _nmapParser.ParseNmapXml(stream);
                    importedFindings.AddRange(parsedNmap.Select(p => new ImportedFindingDto
                    {
                        Title = p.Title,
                        Description = p.Description,
                        Source = p.Source,
                        SourceSeverity = p.SourceSeverity
                    }));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Failed to parse file: {ex.Message}" });
            }
            if (!importedFindings.Any())
                return BadRequest(new { message = "No valid findings found in the file." });
            var newFindingsCount = 0;
            var duplicateCount = 0;
            foreach (var dto in importedFindings)
            {
                if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.SourceSeverity)) continue;
                var existingFinding = await _context.Findings.FirstOrDefaultAsync(f => f.AssetId == assetId && f.Title == dto.Title && f.Source == dto.Source);
                if (existingFinding != null)
                {
                    _context.FindingOccurrences.Add(new FindingOccurrence(existingFinding.Id, "Imported via batch scanner report."));
                    duplicateCount++;
                }
                else
                {
                    var finding = new Finding(assetId, dto.Title, dto.Description, dto.Source, dto.SourceSeverity, dto.ExternalId);
                    _context.Findings.Add(finding);
                    _context.FindingOccurrences.Add(new FindingOccurrence(finding.Id, "Initial import creation."));
                    newFindingsCount++;
                }
            }
            await _context.SaveChangesAsync();
            return Ok(new { message = "Import completed successfully.", newFindingsAdded = newFindingsCount, existingFindingsUpdated = duplicateCount });
        }
    }
}
