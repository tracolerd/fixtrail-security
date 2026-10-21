using System.ComponentModel.DataAnnotations;
namespace FixTrailSecurity.Api.DTOs
{
    public class CreateAssetRequest
    {
        [Required(ErrorMessage = "Asset name is required.")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(100)]
        public string Environment { get; set; } = string.Empty;
        [MaxLength(100)]
        public string Owner { get; set; } = string.Empty;
    }
}
