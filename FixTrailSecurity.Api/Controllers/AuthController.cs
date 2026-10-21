using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FixTrailSecurity.Api.DTOs;
using FixTrailSecurity.Core.Constants;
using FixTrailSecurity.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
namespace FixTrailSecurity.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        public AuthController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }
        [HttpPost("bootstrap")]
        public async Task<IActionResult> BootstrapAdmin([FromBody] BootstrapAdminRequest request)
        {
            // Secure bootstrap: Only runs if NO admin exists in the system
            var adminRoleExists = await _roleManager.RoleExistsAsync(RoleConstants.Admin);
            if (adminRoleExists)
            {
                var admins = await _userManager.GetUsersInRoleAsync(RoleConstants.Admin);
                if (admins.Any()) return BadRequest(new { message = "Bootstrap failed: Administrator already exists." });
            }
            // Seed Roles
            string[] roleNames = { RoleConstants.Admin, RoleConstants.SecurityAnalyst, RoleConstants.RemediationOwner, RoleConstants.ReadOnly };
            foreach (var role in roleNames)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole(role));
            }
            var user = new ApplicationUser { UserName = request.Email, Email = request.Email, FullName = request.FullName };
            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, RoleConstants.Admin);
                return Ok(new { message = "Bootstrap successful. Initial Admin account created." });
            }
            return BadRequest(result.Errors);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
                return Unauthorized(new { message = "Invalid email or password." });
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.FullName)
            };
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));
            var secret = _configuration["JwtSettings:Secret"];
            if (string.IsNullOrEmpty(secret)) throw new InvalidOperationException("JWT Secret is not configured.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_configuration["JwtSettings:ExpiryMinutes"]!)),
                signingCredentials: creds
            );
            return Ok(new AuthResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Email = user.Email!,
                FullName = user.FullName,
                Roles = roles
            });
        }
    }
}
