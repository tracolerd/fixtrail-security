using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FixTrailSecurity.Api.DTOs;
using FixTrailSecurity.Core.Constants;
using FixTrailSecurity.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Data;
using FixTrailSecurity.Core.Data;
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
        private readonly AppDbContext _context;
        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = context;
        }
        [HttpPost("bootstrap")]
        public async Task<IActionResult> BootstrapAdmin([FromBody] BootstrapAdminRequest request)
        {
            // Disable bootstrap unless an operator explicitly configures a secret.
            var expectedSecret = _configuration["BootstrapSettings:Secret"];
            if (string.IsNullOrWhiteSpace(expectedSecret))
                return NotFound(new { message = "Bootstrap is explicitly disabled." });

            if (!Request.Headers.TryGetValue("X-Bootstrap-Secret", out var providedSecret))
                return Unauthorized(new { message = "Invalid bootstrap secret." });

            var expectedBytes = Encoding.UTF8.GetBytes(expectedSecret);
            var providedBytes = Encoding.UTF8.GetBytes(providedSecret.ToString());
            if (!CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes))
                return Unauthorized(new { message = "Invalid bootstrap secret." });

            // Identity managers and AppDbContext share the request-scoped EF
            // context. Serializable isolation prevents concurrent bootstrap
            // requests from both committing after observing zero admins.
            await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                var admins = await _userManager.GetUsersInRoleAsync(RoleConstants.Admin);
                if (admins.Count > 0)
                {
                    await transaction.RollbackAsync();
                    return Conflict(new { message = "Bootstrap is already complete; an administrator exists." });
                }

                string[] roleNames =
                {
                    RoleConstants.Admin,
                    RoleConstants.SecurityAnalyst,
                    RoleConstants.RemediationOwner,
                    RoleConstants.ReadOnly
                };

                foreach (var role in roleNames)
                {
                    if (await _roleManager.RoleExistsAsync(role))
                        continue;

                    var roleResult = await _roleManager.CreateAsync(new IdentityRole(role));
                    if (!roleResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        return Conflict(new
                        {
                            message = "Bootstrap could not initialize required roles.",
                            errors = roleResult.Errors.Select(error => error.Code)
                        });
                    }
                }

                var user = new ApplicationUser
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FullName = request.FullName
                };
                var userResult = await _userManager.CreateAsync(user, request.Password);
                if (!userResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(userResult.Errors);
                }

                var adminResult = await _userManager.AddToRoleAsync(user, RoleConstants.Admin);
                if (!adminResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    return Conflict(new
                    {
                        message = "Bootstrap could not assign the administrator role.",
                        errors = adminResult.Errors.Select(error => error.Code)
                    });
                }

                await transaction.CommitAsync();
                return Ok(new
                {
                    message = "Bootstrap successful. Initial Admin account created. Disable BootstrapSettings:Secret now."
                });
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();
                return Conflict(new { message = "Concurrent bootstrap or database conflict; no partial bootstrap was committed." });
            }
            catch (InvalidOperationException)
            {
                await transaction.RollbackAsync();
                return Conflict(new { message = "Bootstrap could not complete because another initialization operation is in progress." });
            }
        }
        [HttpPost("login")]
        [EnableRateLimiting("LoginPolicy")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) return Unauthorized(new { message = "Invalid email or password." });
            if (await _userManager.IsLockedOutAsync(user))
                return StatusCode(423, new { message = "Account locked out due to multiple failed attempts." });
            if (!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                await _userManager.AccessFailedAsync(user);
                return Unauthorized(new { message = "Invalid email or password." });
            }
            await _userManager.ResetAccessFailedCountAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.FullName)
            };
            foreach (var role in roles) claims.Add(new Claim(ClaimTypes.Role, role));
            var secret = _configuration["JwtSettings:Secret"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!));
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
