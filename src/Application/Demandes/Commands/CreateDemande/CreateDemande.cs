using IdaraTech.Application.Common.Interfaces;
using IdaraTech.Domain.Entities;
using IdaraTech.Domain.Events;

namespace IdaraTech.Application.Demandes.Commands.CreateDemande;

public record CreateDemandeCommand : IRequest<int>
{
    public int CitoyenId { get; init; }

    public string Description { get; init; } = string.Empty;

    public int? ServiceId { get; init; }
}

public class CreateDemandeCommandHandler : IRequestHandler<CreateDemandeCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateDemandeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateDemandeCommand request, CancellationToken cancellationToken)
    {
        var entity = new Demande
        {
            CitoyenId = request.CitoyenId,
            Description = request.Description,
            ServiceId = request.ServiceId,
            Statut = Domain.Enums.Statut.EnAttente
        };

        entity.AddDomainEvent(new DemandeCreatedEvent(entity));

        _context.Demandes.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
