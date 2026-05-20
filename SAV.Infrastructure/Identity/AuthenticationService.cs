using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using SAV.Domain.Entities;
using SAV.Infrastructure.Persistence;
using SAV.Shared.Interfaces;
using SAV.Shared.DTOs.Auth;

namespace SAV.Infrastructure.Identity;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly SavDbContext _context;

    public AuthenticationService(
        UserManager<User> userManager,
        RoleManager<IdentityRole<int>> roleManager,
        IJwtTokenService jwtTokenService,
        ILogger<AuthenticationService> logger,
        SavDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
        _context = context;
    }

    public async Task<object?> LoginAsync(object request)
    {
        if (request is not LoginRequest loginRequest)
            return null;

        var user = await _userManager.FindByEmailAsync(loginRequest.Email);
        if (user == null || !user.IsActive)
            return null;

        var passwordValid = await _userManager.CheckPasswordAsync(user, loginRequest.Password);
        if (!passwordValid)
            return null;

        var roles = await _userManager.GetRolesAsync(user);
        var roleName = roles.FirstOrDefault() ?? "Client";

        var accessToken = _jwtTokenService.GenerateAccessToken(user, roleName);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            UserId = user.Id,
            FullName = user.FullName,
            Role = roleName
        };
    }

    public async Task<bool> RegisterAsync(object request, string roleName = "Client")
    {
        if (request is not RegisterRequest registerRequest)
            return false;

        var userExists = await _userManager.FindByEmailAsync(registerRequest.Email);
        if (userExists != null)
            return false;

        var user = new User
        {
            UserName = registerRequest.Email,
            Email = registerRequest.Email,
            FullName = registerRequest.FullName,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, registerRequest.Password);
        if (!result.Succeeded)
            return false;

        if (!await _roleManager.RoleExistsAsync(roleName))
            await _roleManager.CreateAsync(new IdentityRole<int> { Name = roleName });

        await _userManager.AddToRoleAsync(user, roleName);
        return true;
    }

    public async Task<object?> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            // Find the refresh token in the database
            var token = await _context.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == refreshToken
                                      && !t.IsRevoked
                                      && t.ExpiresAt > DateTime.UtcNow);

            if (token == null)
            {
                _logger.LogWarning("Invalid or expired refresh token attempted.");
                return null;
            }

            // Revoke the old refresh token
            token.IsRevoked = true;
            _context.RefreshTokens.Update(token);

            // Generate new access token
            var roles = await _userManager.GetRolesAsync(token.User);
            var roleName = roles.FirstOrDefault() ?? "Client";
            var newAccessToken = _jwtTokenService.GenerateAccessToken(token.User, roleName);

            // Generate new refresh token
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

            // Save new refresh token to database
            _context.RefreshTokens.Add(new RefreshToken
            {
                Token = newRefreshToken,
                UserId = token.UserId,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            });

            await _context.SaveChangesAsync();

            return new LoginResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                UserId = token.User.Id,
                FullName = token.User.FullName,
                Role = roleName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh.");
            return null;
        }
    }

    public async Task LogoutAsync(int userId)
    {
        try
        {
            // Revoke all refresh tokens for the user
            var userTokens = await _context.RefreshTokens
                .Where(t => t.UserId == userId && !t.IsRevoked)
                .ToListAsync();

            foreach (var token in userTokens)
            {
                token.IsRevoked = true;
            }

            if (userTokens.Count > 0)
            {
                _context.RefreshTokens.UpdateRange(userTokens);
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation($"User {userId} logged out successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error during logout for user {userId}.");
        }
    }

    /// <summary>
    /// Creates an admin user account with the Admin role.
    /// </summary>
    public async Task<bool> CreateAdminAsync(string email, string password, string fullName)
    {
        try
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                _logger.LogWarning($"Admin user with email {email} already exists.");
                return false;
            }

            // Create the admin user
            var adminUser = new User
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(adminUser, password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError($"Failed to create admin user: {errors}");
                return false;
            }

            // Create Admin role if it doesn't exist
            const string adminRole = "Admin";
            if (!await _roleManager.RoleExistsAsync(adminRole))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole<int> { Name = adminRole });
                if (!roleResult.Succeeded)
                {
                    _logger.LogError("Failed to create Admin role.");
                    return false;
                }
            }

            // Assign Admin role to the user
            var addRoleResult = await _userManager.AddToRoleAsync(adminUser, adminRole);
            if (!addRoleResult.Succeeded)
            {
                _logger.LogError("Failed to assign Admin role to user.");
                return false;
            }

            _logger.LogInformation($"Admin user created successfully: {email}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating admin user.");
            return false;
        }
    }
}
