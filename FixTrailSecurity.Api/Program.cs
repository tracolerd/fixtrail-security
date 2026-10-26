using System.Text;
using FixTrailSecurity.Core.Constants;
using FixTrailSecurity.Core.Data;
using FixTrailSecurity.Core.Entities;
using FixTrailSecurity.Core.Services;
using FixTrailSecurity.Core.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAngularClient", p => p.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod());
});
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("YOUR_DB_PASSWORD_HERE")) {
    throw new InvalidOperationException("Database connection string is not properly configured. Please use .NET User Secrets to set 'ConnectionStrings:DefaultConnection'.");
}
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, b => b.MigrationsAssembly("FixTrailSecurity.Api")));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();
var jwtSecret = JwtSecretValidator.RequireStrongSecret(
    builder.Configuration["JwtSettings:Secret"]);
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization(options => {
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole(RoleConstants.Admin));
    options.AddPolicy("RequireAnalystOrAdmin", policy => policy.RequireRole(RoleConstants.Admin, RoleConstants.SecurityAnalyst));
    options.AddPolicy("RequireRemediationOwner", policy => policy.RequireRole(RoleConstants.Admin, RoleConstants.SecurityAnalyst, RoleConstants.RemediationOwner));
});
builder.Services.AddRateLimiter(options => {
    options.AddFixedWindowLimiter("LoginPolicy", opt => {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;
    });
});
builder.Services.AddScoped<NmapXmlParserService>();
var app = builder.Build();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseHttpsRedirection();
app.UseRateLimiter();
app.Use(async (context, next) => {
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    await next();
});
app.UseCors("AllowAngularClient");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
