using IdaraTech.Application.Common.Interfaces;

namespace IdaraTech.Application.Prestataires.Commands.DeletePrestataire;

public record DeletePrestataireCommand(int Id) : IRequest;

public class DeletePrestataireCommandHandler : IRequestHandler<DeletePrestataireCommand>
{
    private readonly IApplicationDbContext _context;

    public DeletePrestataireCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeletePrestataireCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Prestataires
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        _context.Prestataires.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
