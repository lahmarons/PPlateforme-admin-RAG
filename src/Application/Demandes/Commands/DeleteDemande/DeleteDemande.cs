using IdaraTech.Application.Common.Interfaces;

namespace IdaraTech.Application.Demandes.Commands.DeleteDemande;

public record DeleteDemandeCommand(int Id) : IRequest;

public class DeleteDemandeCommandHandler : IRequestHandler<DeleteDemandeCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteDemandeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteDemandeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Demandes
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        _context.Demandes.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
