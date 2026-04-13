using IdaraTech.Application.Common.Interfaces;
using IdaraTech.Application.Common.Exceptions;
using IdaraTech.Domain.Entities;
using IdaraTech.Domain.Events;
using IdaraTech.Domain.Enums;

namespace IdaraTech.Application.Demandes.Commands.ChangeDemandeStatus;

public record ChangeDemandeStatusCommand : IRequest
{
    public int DemandeId { get; init; }

    public Statut NewStatus { get; init; }
}

public class ChangeDemandeStatusCommandHandler : IRequestHandler<ChangeDemandeStatusCommand>
{
    private readonly IApplicationDbContext _context;

    public ChangeDemandeStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ChangeDemandeStatusCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Demandes
            .FindAsync(new object[] { request.DemandeId }, cancellationToken: cancellationToken);

        if (entity is null)
        {
            throw new IdaraTech.Application.Common.Exceptions.NotFoundException(nameof(Demande), request.DemandeId);
        }

        var previousStatus = entity.Statut;
        entity.ChangerStatut(request.NewStatus);

        entity.AddDomainEvent(new DemandeStatusChangedEvent(entity, previousStatus));

        await _context.SaveChangesAsync(cancellationToken);
    }
}
