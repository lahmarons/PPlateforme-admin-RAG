using System.ComponentModel.DataAnnotations;
using IdaraTech.Application.Common.Interfaces;
using IdaraTech.Domain.Constants;
using IdaraTech.Domain.Entities;
using IdaraTech.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdaraTech.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IApplicationDbContext _context;

    public RegisterModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public string Adresse { get; set; } = string.Empty;

        [Required]
        public string Telephone { get; set; } = string.Empty;
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

        if (await _userManager.FindByEmailAsync(Input.Email) is not null || _context.Citoyens.Any(c => c.Email == Input.Email))
        {
            ErrorMessage = "Un compte existe déjà avec cet email.";
            return Page();
        }

        var user = new ApplicationUser
        {
            UserName = Input.Email,
            Email = Input.Email
        };

        var result = await _userManager.CreateAsync(user, Input.Password);
        if (!result.Succeeded)
        {
            ErrorMessage = string.Join(" ", result.Errors.Select(error => error.Description));
            return Page();
        }

        await _userManager.AddToRoleAsync(user, Roles.Citoyen);

        _context.Citoyens.Add(new Citoyen
        {
            Nom = Input.Nom,
            Email = Input.Email,
            MotDePasse = user.PasswordHash ?? string.Empty,
            Adresse = Input.Adresse,
            Telephone = Input.Telephone
        });
        await _context.SaveChangesAsync(CancellationToken.None);

        await _signInManager.SignInAsync(user, isPersistent: false);
        return Redirect("/citoyen/dashboard");
    }
}
