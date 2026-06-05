using System.ComponentModel.DataAnnotations;
using IdaraTech.Domain.Constants;
using IdaraTech.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdaraTech.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            ErrorMessage = "Identifiants invalides.";
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user is null)
        {
            ErrorMessage = "Identifiants invalides.";
            return Page();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var target = GetRedirectUrl(roles);

        return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? target : returnUrl);
    }

    private static string GetRedirectUrl(IList<string> roles)
    {
        if (roles.Contains(Roles.Administrator))
        {
            return "/admin/dashboard";
        }

        if (roles.Contains(Roles.Agent))
        {
            return "/agent/dashboard";
        }

        return "/citoyen/dashboard";
    }
}
