using System.Security.Claims;
using IdaraTech.Application.Common.Interfaces;
using IdaraTech.Domain.Constants;
using IdaraTech.Domain.Entities;
using IdaraTech.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http.HttpResults;

namespace IdaraTech.Web.Endpoints;

public class Auth : EndpointGroupBase
{
    public override string? GroupName => "auth";

    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost("login", Login).AllowAnonymous();
        groupBuilder.MapPost("register/citoyen", RegisterCitoyen).AllowAnonymous();
        groupBuilder.MapGet("me", Me).RequireAuthorization();
        groupBuilder.MapPost("logout", Logout).RequireAuthorization();
    }

    public async Task<IResult> Login(
        LoginRequest request,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        var result = await signInManager.PasswordSignInAsync(request.Email, request.Password, isPersistent: false, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            return TypedResults.Unauthorized();
        }

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return TypedResults.Unauthorized();
        }

        var roles = await userManager.GetRolesAsync(user);
        return TypedResults.Ok(new AuthResponse(user.Id, user.UserName ?? request.Email, roles.ToArray(), GetRedirectUrl(roles)));
    }

    public async Task<IResult> RegisterCitoyen(
        RegisterCitoyenRequest request,
        UserManager<ApplicationUser> userManager,
        IApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null || await context.Citoyens.AnyAsync(c => c.Email == request.Email, cancellationToken))
        {
            return TypedResults.Conflict("Un compte existe déjà avec cet email.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = false
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return TypedResults.BadRequest(string.Join(" ", createResult.Errors.Select(error => error.Description)));
        }

        var roleResult = await userManager.AddToRoleAsync(user, Roles.Citoyen);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return TypedResults.BadRequest(string.Join(" ", roleResult.Errors.Select(error => error.Description)));
        }

        context.Citoyens.Add(new Citoyen
        {
            Nom = request.Nom,
            Email = request.Email,
            MotDePasse = user.PasswordHash ?? string.Empty,
            Adresse = request.Adresse,
            Telephone = request.Telephone
        });

        await context.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(new AuthResponse(user.Id, user.UserName ?? request.Email, [Roles.Citoyen], "/citoyen/dashboard"));
    }

    public async Task<Ok<AuthResponse?>> Me(UserManager<ApplicationUser> userManager, ClaimsPrincipal principal, IApplicationDbContext context)
    {
        var user = await userManager.GetUserAsync(principal);
        if (user is null)
        {
            return TypedResults.Ok<AuthResponse?>(null);
        }

        var roles = await userManager.GetRolesAsync(user);
        
        int? citoyenId = null;
        if (roles.Contains(Roles.Citoyen))
        {
            var citoyen = await context.Citoyens.FirstOrDefaultAsync(c => c.Email == user.Email);
            citoyenId = citoyen?.Id;
        }
        
        return TypedResults.Ok<AuthResponse?>(new AuthResponse(user.Id, user.UserName ?? string.Empty, roles.ToArray(), GetRedirectUrl(roles), citoyenId));
    }

    public async Task<IResult> Logout(SignInManager<ApplicationUser> signInManager)
    {
        await signInManager.SignOutAsync();
        return TypedResults.NoContent();
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

public record LoginRequest(string Email, string Password);

public record RegisterCitoyenRequest(string Email, string Password, string Nom, string Adresse, string Telephone);

public record AuthResponse(string UserId, string UserName, string[] Roles, string RedirectUrl, int? CitoyenId = null);
