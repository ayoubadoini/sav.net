using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MediatR;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using SAV.Infrastructure.Persistence;
using SAV.Infrastructure.Identity;
using SAV.Infrastructure.Repositories;
using SAV.Shared.Interfaces;
using SAV.Shared.Constants;
using SAV.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// SERVICES CONFIGURATION
// ============================================================

// Add services
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

// ============================================================
// DATABASE CONFIGURATION
// ============================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=(localdb)\\mssqllocaldb;Database=SAVDb;Integrated Security=true;";
builder.Services.AddDbContext<SavDbContext>(options =>
    options.UseSqlServer(connectionString));

// ============================================================
// AUTHENTICATION & AUTHORIZATION CONFIGURATION
// ============================================================

// Identity
builder.Services.AddIdentityCore<SAV.Domain.Entities.User>()
    .AddRoles<Microsoft.AspNetCore.Identity.IdentityRole<int>>()
    .AddEntityFrameworkStores<SavDbContext>();

// Configure password policy for easier testing
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
});

// JWT Settings
var jwtSettings = new JwtSettings
{
    Key = builder.Configuration["JwtSettings:Key"] ?? "your-secret-key-min-32-characters-very-very-long",
    Issuer = builder.Configuration["JwtSettings:Issuer"] ?? "SAVApp",
    Audience = builder.Configuration["JwtSettings:Audience"] ?? "SAVAppUsers",
    ExpirationMinutes = int.Parse(builder.Configuration["JwtSettings:ExpirationMinutes"] ?? "60"),
    RefreshTokenExpirationDays = int.Parse(builder.Configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7")
};

builder.Services.AddSingleton(jwtSettings);

// JWT Services
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// JWT Bearer Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        // Allow SignalR to receive token from query string for websocket/negotiate requests
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"].FirstOrDefault();
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// ============================================================
// AUTHORIZATION POLICIES CONFIGURATION
// ============================================================

builder.Services.AddAuthorization(options =>
{
    // Admin-only policy
    options.AddPolicy(Policies.AdminOnly, policy =>
        policy.RequireRole(Roles.Admin));

    // Technician-only policy
    options.AddPolicy(Policies.TechnicianOnly, policy =>
        policy.RequireRole(Roles.Technician));

    // Client-only policy
    options.AddPolicy(Policies.ClientOnly, policy =>
        policy.RequireRole(Roles.Client));

    // Admin or Technician policy
    options.AddPolicy(Policies.AdminOrTechnician, policy =>
        policy.RequireRole(Roles.Admin, Roles.Technician));

    // Admin or Client policy
    options.AddPolicy(Policies.AdminOrClient, policy =>
        policy.RequireRole(Roles.Admin, Roles.Client));

    // Authenticated users only (default)
    options.AddPolicy(Policies.Authenticated, policy =>
        policy.RequireAuthenticatedUser());
});

// ============================================================
// REPOSITORIES & UNIT OF WORK
// ============================================================

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ============================================================
// MEDIATOR CONFIGURATION
// ============================================================

builder.Services.AddMediatR(config =>
    config.RegisterServicesFromAssembly(typeof(SAV.Application.Features.Auth.Commands.LoginCommand).Assembly)
);

// ============================================================
// CORS CONFIGURATION
// ============================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebUI", policy =>
        policy
            .WithOrigins(
                "http://localhost:5050",
                "https://localhost:7105",
                "http://localhost:3000",
                "http://localhost:3001"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

// ============================================================
// BUILD APPLICATION
// ============================================================

var app = builder.Build();

// ============================================================
// SEED ADMIN USER ON STARTUP
// ============================================================

using (var scope = app.Services.CreateScope())
{
    var authService = scope.ServiceProvider.GetRequiredService<IAuthenticationService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        // Create default admin account if it doesn't exist
        var adminCreated = await authService.CreateAdminAsync(
            email: "admin@savmanager.com",
            password: "Admin@123456",
            fullName: "System Administrator"
        );

        if (adminCreated)
        {
            logger.LogInformation("✅ Default admin account created successfully.");
        }
        else
        {
            logger.LogInformation("ℹ️  Admin account already exists or creation was skipped.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Error seeding admin user during startup.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SAV Manager API v1");
        c.DefaultModelsExpandDepth(-1);
    });
}

// HTTPS redirection intentionally disabled for local development to avoid
// HTTP -> HTTPS 307 redirects which drop Authorization headers during
// redirects from the dev server. If you need strict HTTPS in production,
// re-enable `app.UseHttpsRedirection()` behind a non-development check.
app.UseCors("AllowWebUI");
app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");
app.MapFallbackToFile("index.html");

app.Run();


