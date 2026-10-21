using Microsoft.AspNetCore.Identity;
namespace FixTrailSecurity.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
    }
}
