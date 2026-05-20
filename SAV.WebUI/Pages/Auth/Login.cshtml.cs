using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SAV.WebUI.Models;
using SAV.WebUI.Services;

namespace SAV.WebUI.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly IAuthService _authService;
    public string? ErrorMessage { get; set; }

    [BindProperty]
    public LoginRequest Input { get; set; } = new();

    public LoginModel(IAuthService authService)
    {
        _authService = authService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _authService.LoginAsync(Input);
        if (result != null)
        {
            HttpContext.Session.SetString("AuthToken", result.Token);
            return RedirectToPage("/Index");
        }

        ErrorMessage = "Email ou mot de passe incorrect";
        return Page();
    }
}
