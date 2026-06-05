using IdaraTech.Application.Common.Interfaces;
using IdaraTech.Domain.Entities;
using IdaraTech.Domain.Events;

namespace IdaraTech.Application.Demandes.Commands.CreateDemande;

public record DocumentUploadDto(string Nom, string Type, string ContenuBase64);

public record CreateDemandeCommand : IRequest<int>
{
    public int CitoyenId { get; init; }

    public string Description { get; init; } = string.Empty;

    public int? ServiceId { get; init; }

    public List<DocumentUploadDto> Documents { get; init; } = new();
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

        foreach (var d in request.Documents)
        {
            entity.Documents.Add(new Document
            {
                Nom = d.Nom,
                Type = d.Type,
                Contenu = Convert.FromBase64String(d.ContenuBase64)
            });
        }

        entity.AddDomainEvent(new DemandeCreatedEvent(entity));

        _context.Demandes.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
