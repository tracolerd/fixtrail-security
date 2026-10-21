using FixTrailSecurity.Api.DTOs;
using FixTrailSecurity.Core.Data;
using FixTrailSecurity.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FixTrailSecurity.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AssetsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AssetsController(AppDbContext context) { _context = context; }
        [HttpPost]
        [Authorize(Policy = "RequireAnalystOrAdmin")]
        public async Task<IActionResult> CreateAsset([FromBody] CreateAssetRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var asset = new Asset(request.Name, request.Environment, request.Owner);
            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAsset), new { id = asset.Id }, asset);
        }
        [HttpGet]
        public async Task<IActionResult> GetAssets()
        {
            var assets = await _context.Assets.ToListAsync();
            return Ok(assets);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsset(Guid id)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null) return NotFound();
            return Ok(asset);
        }
    }
}
