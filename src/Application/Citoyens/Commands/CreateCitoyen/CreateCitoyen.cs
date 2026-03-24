using IdaraTech.Application.Common.Interfaces;
using IdaraTech.Domain.Entities;

namespace IdaraTech.Application.Citoyens.Commands.CreateCitoyen;

public record CreateCitoyenCommand : IRequest<int>
{
    public string Nom { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string MotDePasse { get; init; } = string.Empty;
    public string Adresse { get; init; } = string.Empty;
    public string Telephone { get; init; } = string.Empty;
}

public class CreateCitoyenCommandHandler : IRequestHandler<CreateCitoyenCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateCitoyenCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateCitoyenCommand request, CancellationToken cancellationToken)
    {
        var entity = new Citoyen
        {
            Nom = request.Nom,
            Email = request.Email,
            MotDePasse = request.MotDePasse,
            Adresse = request.Adresse,
            Telephone = request.Telephone
        };

        _context.Citoyens.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
