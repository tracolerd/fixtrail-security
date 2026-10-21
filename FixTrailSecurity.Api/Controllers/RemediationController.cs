using System.Security.Claims;
using FixTrailSecurity.Api.DTOs;
using FixTrailSecurity.Core.Data;
using FixTrailSecurity.Core.Entities;
using FixTrailSecurity.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace FixTrailSecurity.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/findings/{findingId}/remediation")]
    public class RemediationController : ControllerBase
    {
        private readonly AppDbContext _context;
        public RemediationController(AppDbContext context)
        {
            _context = context;
        }
        [HttpPost("assign")]
        [Authorize(Policy = "RequireAnalystOrAdmin")]
        public async Task<IActionResult> AssignTask(Guid findingId, [FromBody] AssignTaskRequest request)
        {
            var finding = await _context.Findings.FindAsync(findingId);
            if (finding == null) return NotFound(new { message = "Finding not found." });
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? "Unknown User";
            var task = new RemediationTask(finding.Id, request.AssignedTo, request.DueDate, request.Notes);
            _context.RemediationTasks.Add(task);
            finding.UpdateStatus(FindingStatus.Assigned);
            var audit = new AuditEvent(finding.Id, "Assigned", $"Assigned to {request.AssignedTo}", userEmail);
            _context.AuditEvents.Add(audit);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Task assigned successfully.", taskId = task.Id });
        }
        [HttpPost("tasks/{taskId}/evidence")]
        [Authorize(Policy = "RequireRemediationOwner")]
        public async Task<IActionResult> SubmitEvidence(Guid findingId, Guid taskId, [FromBody] SubmitEvidenceRequest request)
        {
            var finding = await _context.Findings.FindAsync(findingId);
            var task = await _context.RemediationTasks.FirstOrDefaultAsync(t => t.Id == taskId && t.FindingId == findingId);
            if (finding == null || task == null) return NotFound(new { message = "Finding or Task not found." });
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? "Unknown User";
            var evidence = new EvidenceReference(task.Id, request.EvidenceUrlOrPath, request.Description);
            _context.EvidenceReferences.Add(evidence);
            finding.UpdateStatus(FindingStatus.Mitigated);
            var audit = new AuditEvent(finding.Id, "EvidenceSubmitted", "Evidence provided. Awaiting retest.", userEmail);
            _context.AuditEvents.Add(audit);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Evidence submitted successfully." });
        }
        [HttpPost("retest")]
        [Authorize(Policy = "RequireAnalystOrAdmin")]
        public async Task<IActionResult> RetestFinding(Guid findingId, [FromBody] RetestFindingRequest request)
        {
            var finding = await _context.Findings.FindAsync(findingId);
            if (finding == null) return NotFound(new { message = "Finding not found." });
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? "Unknown User";
            var retest = new RetestRecord(finding.Id, request.IsSuccessful, userEmail, request.Notes);
            _context.RetestRecords.Add(retest);
            var newStatus = request.IsSuccessful ? FindingStatus.Closed : FindingStatus.Reopened;
            finding.UpdateStatus(newStatus);
            var auditAction = request.IsSuccessful ? "RetestPassed" : "RetestFailed";
            var audit = new AuditEvent(finding.Id, auditAction, $"Retest conducted. Status changed to {newStatus}", userEmail);
            _context.AuditEvents.Add(audit);
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Retest completed. Finding is now {newStatus}." });
        }
    }
}
