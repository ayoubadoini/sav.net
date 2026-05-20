using MediatR;
using SAV.Application.DTOs.Auth;
using SAV.Shared.Interfaces;

namespace SAV.Application.Features.Auth.Commands;

public class LoginCommand : IRequest<LoginResponse>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IAuthenticationService _authService;

    public LoginCommandHandler(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var loginRequest = new LoginRequest { Email = request.Email, Password = request.Password };
        var result = await _authService.LoginAsync(loginRequest);

        if (result is SAV.Shared.DTOs.Auth.LoginResponse sharedResponse)
        {
            return new LoginResponse
            {
                AccessToken = sharedResponse.AccessToken,
                RefreshToken = sharedResponse.RefreshToken,
                UserId = sharedResponse.UserId,
                FullName = sharedResponse.FullName,
                Role = sharedResponse.Role
            };
        }

        return new LoginResponse();
    }
}
