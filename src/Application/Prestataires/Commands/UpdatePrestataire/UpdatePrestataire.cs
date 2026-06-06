using IdaraTech.Application.Common.Interfaces;

namespace IdaraTech.Application.Prestataires.Commands.UpdatePrestataire;

public record UpdatePrestataireCommand : IRequest
{
    public int Id { get; init; }
    public string Nom { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Logo { get; init; }
}

public class UpdatePrestataireCommandHandler : IRequestHandler<UpdatePrestataireCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdatePrestataireCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdatePrestataireCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Prestataires
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Nom = request.Nom;
        entity.Description = request.Description;
        entity.Logo = request.Logo;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
