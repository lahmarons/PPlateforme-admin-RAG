using IdaraTech.Application.Common.Interfaces;

namespace IdaraTech.Application.Admins.Commands.UpdateAdmin;

public record UpdateAdminCommand : IRequest
{
    public int Id { get; init; }
    public string Nom { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string MotDePasse { get; init; } = string.Empty;
}

public class UpdateAdminCommandHandler : IRequestHandler<UpdateAdminCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateAdminCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateAdminCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Admins
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Nom = request.Nom;
        entity.Email = request.Email;
        entity.MotDePasse = request.MotDePasse;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
