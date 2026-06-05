using IdaraTech.Domain.Constants;
using IdaraTech.Domain.Entities;
using IdaraTech.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IdaraTech.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}

public class ApplicationDbContextInitialiser
{
    private const string AdministratorEmail = "administrator@localhost";
    private const string AdministratorPassword = "Administrator1!";
    private const string AgentEmail = "agent@localhost";
    private const string AgentPassword = "Agent123!";

    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            // See https://jasontaylor.dev/ef-core-database-initialisation-strategies
            // Skip EnsureDeletedAsync in development to preserve existing data
            // await _context.Database.EnsureDeletedAsync();
            if (await _context.Database.CanConnectAsync())
            {
                var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();
                if (!appliedMigrations.Any())
                {
                    var databaseCreator = _context.Database.GetService<IRelationalDatabaseCreator>();
                    if (databaseCreator.Exists())
                    {
                        await _context.Database.EnsureDeletedAsync();
                    }
                }
            }

            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        var roles = new[]
        {
            Roles.Citoyen,
            Roles.Agent,
            Roles.Administrator
        };

        foreach (var roleName in roles)
        {
            if (await _roleManager.FindByNameAsync(roleName) is null)
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        await SeedIdentityUserAsync(AdministratorEmail, AdministratorPassword, Roles.Administrator);
        await SeedIdentityUserAsync(AgentEmail, AgentPassword, Roles.Agent);

        await SeedDomainAdminAsync();
        await SeedDomainAgentAsync();
    }

    private async Task SeedIdentityUserAsync(string email, string password, string role)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(string.Join(" ", result.Errors.Select(error => error.Description)));
            }
        }

        if (!await _userManager.IsInRoleAsync(user, role))
        {
            await _userManager.AddToRoleAsync(user, role);
        }
    }

    private async Task SeedDomainAdminAsync()
    {
        if (!_context.Admins.Any(admin => admin.Email == AdministratorEmail))
        {
            _context.Admins.Add(new Admin
            {
                Nom = "Administrateur",
                Email = AdministratorEmail,
                MotDePasse = AdministratorPassword
            });

            await _context.SaveChangesAsync();
        }
    }

    private async Task SeedDomainAgentAsync()
    {
        if (!_context.Agents.Any(agent => agent.Email == AgentEmail))
        {
            _context.Agents.Add(new Agent
            {
                Nom = "Agent",
                Email = AgentEmail,
                MotDePasse = AgentPassword,
                Matricule = "AG-001",
                Grade = "Senior",
                IsActive = true
            });

            await _context.SaveChangesAsync();
        }
    }
}
