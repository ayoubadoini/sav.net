using SAV.WebUI.Models;

namespace SAV.WebUI.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<bool> RegisterAsync(RegisterRequest request);
    void SetToken(string token);
    string? GetToken();
}

public class AuthService : IAuthService
{
    private readonly IApiService _apiService;

    public AuthService(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var response = await _apiService.PostAsync<LoginResponse>("/api/auth/login", request);
        if (response != null)
        {
            SetToken(response.Token);
        }
        return response;
    }

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        var response = await _apiService.PostAsync<object>("/api/auth/register", request);
        return response != null;
    }

    public void SetToken(string token)
    {
        _apiService.SetAuthToken(token);
    }

    public string? GetToken()
    {
        return null; // À implémenter avec le stockage
    }
}
