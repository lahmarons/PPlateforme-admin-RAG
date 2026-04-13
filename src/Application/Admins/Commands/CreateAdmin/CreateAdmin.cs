using IdaraTech.Application.Common.Interfaces;
using IdaraTech.Domain.Entities;

namespace IdaraTech.Application.Admins.Commands.CreateAdmin;

public record CreateAdminCommand : IRequest<int>
{
    public string Nom { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string MotDePasse { get; init; } = string.Empty;
}

public class CreateAdminCommandHandler : IRequestHandler<CreateAdminCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateAdminCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateAdminCommand request, CancellationToken cancellationToken)
    {
        var entity = new Admin
        {
            Nom = request.Nom,
            Email = request.Email,
            MotDePasse = request.MotDePasse
        };

        _context.Admins.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
