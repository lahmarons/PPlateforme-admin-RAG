using IdaraTech.Domain.Entities;

namespace IdaraTech.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Utilisateur> Utilisateurs { get; }

    DbSet<Agent> Agents { get; }

    DbSet<Citoyen> Citoyens { get; }

    DbSet<Admin> Admins { get; }

    DbSet<Demande> Demandes { get; }

    DbSet<Service> Services { get; }

    DbSet<Document> Documents { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
