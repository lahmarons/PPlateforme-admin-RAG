using IdaraTech.Application.Common.Interfaces;
using IdaraTech.Domain.Enums;

namespace IdaraTech.Application.Demandes.Commands.UpdateDemande;

public record UpdateDemandeCommand : IRequest
{
    public int Id { get; init; }
    public string Description { get; init; } = string.Empty;
    public int? ServiceId { get; init; }
    public int? AgentId { get; init; }
    public Statut Statut { get; init; }
}

public class UpdateDemandeCommandHandler : IRequestHandler<UpdateDemandeCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateDemandeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateDemandeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Demandes
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Description = request.Description;
        entity.ServiceId = request.ServiceId;
        entity.AgentId = request.AgentId;
        entity.Statut = request.Statut;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
