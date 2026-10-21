using System.ComponentModel.DataAnnotations;
namespace FixTrailSecurity.Api.DTOs
{
    public class ImportedFindingDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [Required]
        public string Source { get; set; } = string.Empty;
        public string? ExternalId { get; set; }
        [Required]
        public string SourceSeverity { get; set; } = string.Empty;
    }
}
