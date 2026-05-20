using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using SAV.Application.DTOs.Auth;
using SAV.Application.Features.Auth.Commands;
using SAV.Shared.Interfaces;
using SAV.Shared.DTOs.Auth;

namespace SAV.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAuthenticationService _authService;

    public AuthController(IMediator mediator, IAuthenticationService authService)
    {
        _mediator = mediator;
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<SAV.Application.DTOs.Auth.LoginResponse>> Login([FromBody] SAV.Application.DTOs.Auth.LoginRequest request)
    {
        var command = new LoginCommand { Email = request.Email, Password = request.Password };
        var result = await _mediator.Send(command);

        if (string.IsNullOrEmpty(result.AccessToken))
            return Unauthorized("Invalid credentials");

        return Ok(result);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<bool>> Register([FromBody] SAV.Application.DTOs.Auth.RegisterRequest request)
    {
        var command = new RegisterCommand
        {
            Email = request.Email,
            Password = request.Password,
            FullName = request.FullName
        };

        var result = await _mediator.Send(command);
        if (!result)
            return BadRequest("Registration failed");

        return Ok(true);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<ActionResult<SAV.Application.DTOs.Auth.LoginResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
            return BadRequest("Refresh token is required");

        var command = new RefreshTokenCommand { RefreshToken = request.RefreshToken };
        var result = await _mediator.Send(command);

        if (string.IsNullOrEmpty(result.AccessToken))
            return Unauthorized("Invalid or expired refresh token");

        return Ok(result);
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    public IActionResult Logout()
    {
        // JWT is stateless - client deletes the token on their side
        // If server-side token revocation is needed, pass userId via query parameter or header
        return Ok(new { message = "Logged out successfully. Please clear your token from the client." });
    }

    /// <summary>
    /// Creates an admin user account (DEVELOPMENT ONLY - should be restricted in production)
    /// </summary>
    [HttpPost("create-admin")]
    [AllowAnonymous]
    public async Task<ActionResult<object>> CreateAdmin([FromBody] CreateAdminRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || 
            string.IsNullOrWhiteSpace(request.Password) || 
            string.IsNullOrWhiteSpace(request.FullName))
        {
            return BadRequest(new { message = "Email, password, and full name are required." });
        }

        var result = await _authService.CreateAdminAsync(request.Email, request.Password, request.FullName);

        if (!result)
            return BadRequest(new { message = "Failed to create admin user. The user may already exist." });

        return Ok(new { message = "Admin user created successfully.", email = request.Email });
    }
}
