using IdaraTech.Application.Common.Interfaces;

namespace IdaraTech.Application.Citoyens.Commands.DeleteCitoyen;

public record DeleteCitoyenCommand(int Id) : IRequest;

public class DeleteCitoyenCommandHandler : IRequestHandler<DeleteCitoyenCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteCitoyenCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteCitoyenCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Citoyens
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        _context.Citoyens.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
