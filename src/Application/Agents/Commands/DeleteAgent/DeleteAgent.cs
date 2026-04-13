using IdaraTech.Application.Common.Interfaces;

namespace IdaraTech.Application.Agents.Commands.DeleteAgent;

public record DeleteAgentCommand(int Id) : IRequest;

public class DeleteAgentCommandHandler : IRequestHandler<DeleteAgentCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteAgentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteAgentCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Agents
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        _context.Agents.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
