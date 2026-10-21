using System;
using System.ComponentModel.DataAnnotations;
namespace FixTrailSecurity.Api.DTOs
{
    public class AssignTaskRequest
    {
        [Required] public string AssignedTo { get; set; } = string.Empty;
        [Required] public DateTime DueDate { get; set; }
        public string? Notes { get; set; }
    }
    public class SubmitEvidenceRequest
    {
        [Required] public string EvidenceUrlOrPath { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
    public class RetestFindingRequest
    {
        [Required] public bool IsSuccessful { get; set; }
        public string? Notes { get; set; }
    }
}
