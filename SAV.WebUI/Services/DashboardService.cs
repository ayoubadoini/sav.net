using SAV.WebUI.Models;

namespace SAV.WebUI.Services;

public interface IDashboardService
{
    Task<DashboardStats?> GetStatsAsync();
}

public class DashboardService : IDashboardService
{
    private readonly IApiService _apiService;

    public DashboardService(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<DashboardStats?> GetStatsAsync()
    {
        return await _apiService.GetAsync<DashboardStats>("/api/dashboard/stats");
    }
}
