using System;
using System.ComponentModel.DataAnnotations;
namespace FixTrailSecurity.Api.DTOs
{
    public class AssignTaskRequest
    {
        [Required]
        public string AssignedTo { get; set; } = string.Empty;
        [Required]
        public DateTime DueDate { get; set; }
        public string? Notes { get; set; }
        [Required]
        public string PerformedBy { get; set; } = string.Empty; // In a real app, this comes from User Claims
    }
    public class SubmitEvidenceRequest
    {
        [Required]
        public string EvidenceUrlOrPath { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required]
        public string PerformedBy { get; set; } = string.Empty;
    }
    public class RetestFindingRequest
    {
        [Required]
        public bool IsSuccessful { get; set; }
        public string? Notes { get; set; }
        [Required]
        public string RetestedBy { get; set; } = string.Empty;
    }
}
