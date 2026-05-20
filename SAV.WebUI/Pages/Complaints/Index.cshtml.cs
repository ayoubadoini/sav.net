using Microsoft.AspNetCore.Mvc.RazorPages;
using SAV.WebUI.Models;
using SAV.WebUI.Services;

namespace SAV.WebUI.Pages.Complaints;

public class IndexModel : PageModel
{
    private readonly IComplaintService _complaintService;
    public List<ComplaintDto> Complaints { get; set; } = new();

    public IndexModel(IComplaintService complaintService)
    {
        _complaintService = complaintService;
    }

    public async Task OnGetAsync()
    {
        var complaints = await _complaintService.GetAllComplaintsAsync();
        Complaints = complaints ?? new List<ComplaintDto>();
    }
}
