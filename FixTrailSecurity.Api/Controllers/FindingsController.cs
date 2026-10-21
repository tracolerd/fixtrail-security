using FixTrailSecurity.Api.DTOs;
using FixTrailSecurity.Core.Data;
using FixTrailSecurity.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FixTrailSecurity.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FindingsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public FindingsController(AppDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> CreateOrUpdateFinding([FromBody] CreateFindingRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var assetExists = await _context.Assets.AnyAsync(a => a.Id == request.AssetId);
            if (!assetExists)
                return NotFound(new { message = "Asset not found." });
            // Deduplication Check
            var existingFinding = await _context.Findings
                .FirstOrDefaultAsync(f => f.AssetId == request.AssetId && f.Title == request.Title && f.Source == request.Source);
            if (existingFinding != null)
            {
                // Record new occurrence instead of creating a duplicate finding
                var occurrence = new FindingOccurrence(existingFinding.Id, "Repeated finding detected during scan.");
                _context.FindingOccurrences.Add(occurrence);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Finding already exists. New occurrence recorded.", findingId = existingFinding.Id });
            }
            // Create new finding
            var finding = new Finding(request.AssetId, request.Title, request.Description, request.Source, request.SourceSeverity, request.ExternalId);
            _context.Findings.Add(finding);
            await _context.SaveChangesAsync();
            // Record initial occurrence
            var initialOccurrence = new FindingOccurrence(finding.Id, "Initial finding creation.");
            _context.FindingOccurrences.Add(initialOccurrence);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetFinding), new { id = finding.Id }, finding);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFinding(Guid id)
        {
            var finding = await _context.Findings.FindAsync(id);
            if (finding == null)
                return NotFound();
            return Ok(finding);
        }
    }
}
