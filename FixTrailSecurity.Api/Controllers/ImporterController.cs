using System.Text.Json;
using FixTrailSecurity.Api.DTOs;
using FixTrailSecurity.Core.Data;
using FixTrailSecurity.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FixTrailSecurity.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImporterController : ControllerBase
    {
        private readonly AppDbContext _context;
        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB Limit
        public ImporterController(AppDbContext context)
        {
            _context = context;
        }
        [HttpPost("upload/{assetId}")]
        public async Task<IActionResult> UploadFindings(Guid assetId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "File is empty or missing." });
            if (file.Length > MaxFileSize)
                return BadRequest(new { message = "File exceeds the 5MB size limit." });
            if (Path.GetExtension(file.FileName).ToLower() != ".json")
                return BadRequest(new { message = "Only JSON files are supported." });
            var assetExists = await _context.Assets.AnyAsync(a => a.Id == assetId);
            if (!assetExists)
                return NotFound(new { message = "Asset not found." });
            List<ImportedFindingDto>? importedFindings;
            try
            {
                using var stream = file.OpenReadStream();
                importedFindings = await JsonSerializer.DeserializeAsync<List<ImportedFindingDto>>(stream, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException)
            {
                return BadRequest(new { message = "Invalid JSON schema. Expected an array of findings." });
            }
            if (importedFindings == null || !importedFindings.Any())
                return BadRequest(new { message = "No valid findings found in the file." });
            var newFindingsCount = 0;
            var duplicateCount = 0;
            foreach (var dto in importedFindings)
            {
                if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.SourceSeverity))
                    continue; // Skip invalid entries
                var existingFinding = await _context.Findings
                    .FirstOrDefaultAsync(f => f.AssetId == assetId && f.Title == dto.Title && f.Source == dto.Source);
                if (existingFinding != null)
                {
                    // Record new occurrence
                    var occurrence = new FindingOccurrence(existingFinding.Id, "Imported via batch JSON.");
                    _context.FindingOccurrences.Add(occurrence);
                    duplicateCount++;
                }
                else
                {
                    // Create new finding
                    var finding = new Finding(assetId, dto.Title, dto.Description, dto.Source, dto.SourceSeverity, dto.ExternalId);
                    _context.Findings.Add(finding);
                    var occurrence = new FindingOccurrence(finding.Id, "Initial import creation.");
                    _context.FindingOccurrences.Add(occurrence);
                    newFindingsCount++;
                }
            }
            await _context.SaveChangesAsync();
            return Ok(new 
            { 
                message = "Import completed successfully.", 
                newFindingsAdded = newFindingsCount, 
                existingFindingsUpdated = duplicateCount 
            });
        }
    }
}
