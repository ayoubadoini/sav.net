using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SAV.WebUI.Models;
using SAV.WebUI.Services;

namespace SAV.WebUI.Pages.Complaints;

public class CreateModel : PageModel
{
    private readonly IComplaintService _complaintService;
    public List<ClientDto> Clients { get; set; } = new();

    [BindProperty]
    public CreateComplaintRequest Input { get; set; } = new();

    public CreateModel(IComplaintService complaintService)
    {
        _complaintService = complaintService;
    }

    public void OnGet()
    {
        // TODO: Load clients from API
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _complaintService.CreateComplaintAsync(Input);
        if (result != null)
        {
            return RedirectToPage("Index");
        }

        ModelState.AddModelError(string.Empty, "Erreur lors de la création de la réclamation");
        return Page();
    }
}
