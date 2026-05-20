using SAV.WebUI.Models;

namespace SAV.WebUI.Services;

public interface IComplaintService
{
    Task<List<ComplaintDto>?> GetAllComplaintsAsync();
    Task<ComplaintDto?> GetComplaintByIdAsync(int id);
    Task<ComplaintDto?> CreateComplaintAsync(CreateComplaintRequest request);
    Task<bool> DeleteComplaintAsync(int id);
}

public class ComplaintService : IComplaintService
{
    private readonly IApiService _apiService;

    public ComplaintService(IApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<List<ComplaintDto>?> GetAllComplaintsAsync()
    {
        return await _apiService.GetAsync<List<ComplaintDto>>("/api/complaints");
    }

    public async Task<ComplaintDto?> GetComplaintByIdAsync(int id)
    {
        return await _apiService.GetAsync<ComplaintDto>($"/api/complaints/{id}");
    }

    public async Task<ComplaintDto?> CreateComplaintAsync(CreateComplaintRequest request)
    {
        return await _apiService.PostAsync<ComplaintDto>("/api/complaints", request);
    }

    public async Task<bool> DeleteComplaintAsync(int id)
    {
        return await _apiService.DeleteAsync($"/api/complaints/{id}");
    }
}
