using System.ComponentModel.DataAnnotations;
namespace FixTrailSecurity.Api.DTOs
{
    public class CreateFindingRequest
    {
        [Required]
        public Guid AssetId { get; set; }
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [Required]
        [MaxLength(100)]
        public string Source { get; set; } = string.Empty;
        [MaxLength(200)]
        public string? ExternalId { get; set; }
        [Required]
        [MaxLength(50)]
        public string SourceSeverity { get; set; } = string.Empty;
    }
}
