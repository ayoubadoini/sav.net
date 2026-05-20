using MediatR;
using SAV.Application.DTOs.Auth;
using SAV.Shared.Interfaces;

namespace SAV.Application.Features.Auth.Commands;

public class RefreshTokenCommand : IRequest<LoginResponse>
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResponse>
{
    private readonly IAuthenticationService _authService;

    public RefreshTokenCommandHandler(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task<LoginResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshTokenAsync(request.RefreshToken);

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
