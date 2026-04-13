using IdaraTech.Application.Common.Interfaces;

namespace IdaraTech.Application.Citoyens.Commands.UpdateCitoyen;

public record UpdateCitoyenCommand : IRequest
{
    public int Id { get; init; }
    public string Nom { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string MotDePasse { get; init; } = string.Empty;
    public string Adresse { get; init; } = string.Empty;
    public string Telephone { get; init; } = string.Empty;
}

public class UpdateCitoyenCommandHandler : IRequestHandler<UpdateCitoyenCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateCitoyenCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateCitoyenCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Citoyens
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Nom = request.Nom;
        entity.Email = request.Email;
        entity.MotDePasse = request.MotDePasse;
        entity.Adresse = request.Adresse;
        entity.Telephone = request.Telephone;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
