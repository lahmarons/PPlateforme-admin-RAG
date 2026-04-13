using System.Reflection;
using IdaraTech.Application.Common.Interfaces;
using IdaraTech.Domain.Entities;
using IdaraTech.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdaraTech.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Utilisateur> Utilisateurs => Set<Utilisateur>();

    public DbSet<Agent> Agents => Set<Agent>();

    public DbSet<Citoyen> Citoyens => Set<Citoyen>();

    public DbSet<Admin> Admins => Set<Admin>();

    public DbSet<Demande> Demandes => Set<Demande>();

    public DbSet<Service> Services => Set<Service>();

    public DbSet<Document> Documents => Set<Document>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
