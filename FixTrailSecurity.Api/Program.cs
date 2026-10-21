using System.Text;
using FixTrailSecurity.Core.Constants;
using FixTrailSecurity.Core.Data;
using FixTrailSecurity.Core.Entities;
using FixTrailSecurity.Core.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAngularClient", p => p.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod());
});
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), b => b.MigrationsAssembly("FixTrailSecurity.Api")));
// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();
// Configure JWT Authentication
var jwtSecret = builder.Configuration["JwtSettings:Secret"];
if (string.IsNullOrEmpty(jwtSecret)) throw new InvalidOperationException("JWT Secret missing.");
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});
// Configure RBAC Policies
builder.Services.AddAuthorization(options => {
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole(RoleConstants.Admin));
    options.AddPolicy("RequireAnalystOrAdmin", policy => policy.RequireRole(RoleConstants.Admin, RoleConstants.SecurityAnalyst));
    options.AddPolicy("RequireRemediationOwner", policy => policy.RequireRole(RoleConstants.Admin, RoleConstants.SecurityAnalyst, RoleConstants.RemediationOwner));
});
builder.Services.AddScoped<NmapXmlParserService>();
var app = builder.Build();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseHttpsRedirection();
// Security Headers
app.Use(async (context, next) => {
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    await next();
});
app.UseCors("AllowAngularClient");
app.UseAuthentication(); // Must be before Authorization
app.UseAuthorization();
app.MapControllers();
app.Run();
