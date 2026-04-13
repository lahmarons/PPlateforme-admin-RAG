using IdaraTech.Application.Common.Interfaces;

namespace IdaraTech.Application.Services.Commands.DeleteService;

public record DeleteServiceCommand(int Id) : IRequest;

public class DeleteServiceCommandHandler : IRequestHandler<DeleteServiceCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteServiceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Services
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        _context.Services.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
