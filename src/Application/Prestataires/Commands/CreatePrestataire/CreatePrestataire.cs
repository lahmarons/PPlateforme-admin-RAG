using IdaraTech.Application.Common.Interfaces;
using IdaraTech.Domain.Entities;

namespace IdaraTech.Application.Prestataires.Commands.CreatePrestataire;

public record CreatePrestataireCommand : IRequest<int>
{
    public string Nom { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? Logo { get; init; }
}

public class CreatePrestataireCommandHandler : IRequestHandler<CreatePrestataireCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreatePrestataireCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreatePrestataireCommand request, CancellationToken cancellationToken)
    {
        var entity = new Prestataire
        {
            Nom = request.Nom,
            Description = request.Description,
            Logo = request.Logo
        };

        _context.Prestataires.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
