using IdaraTech.Application.Common.Interfaces;

namespace IdaraTech.Application.Admins.Commands.DeleteAdmin;

public record DeleteAdminCommand(int Id) : IRequest;

public class DeleteAdminCommandHandler : IRequestHandler<DeleteAdminCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteAdminCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteAdminCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Admins
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        _context.Admins.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
