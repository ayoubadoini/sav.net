using MediatR;
using SAV.Application.DTOs.Auth;
using SAV.Shared.Interfaces;

namespace SAV.Application.Features.Auth.Commands;

public class RegisterCommand : IRequest<bool>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, bool>
{
    private readonly IAuthenticationService _authService;

    public RegisterCommandHandler(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<bool> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var registerRequest = new SAV.Shared.DTOs.Auth.RegisterRequest
        {
            Email = request.Email,
            Password = request.Password,
            FullName = request.FullName
        };
        return await _authService.RegisterAsync(registerRequest, "Client");
    }
}
