using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SAV.WebUI.Models;
using SAV.WebUI.Services;

namespace SAV.WebUI.Pages;

public class IndexModel : PageModel
{
    private readonly IDashboardService _dashboardService;
    public DashboardStats? Stats { get; set; }

    public IndexModel(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task OnGetAsync()
    {
        Stats = await _dashboardService.GetStatsAsync();
        if (Stats == null)
        {
            Stats = new DashboardStats
            {
                TotalComplaints = 0,
                PendingComplaints = 0,
                CompletedComplaints = 0,
                TotalInterventions = 0,
                TotalInvoiceAmount = 0
            };
        }
    }
}

