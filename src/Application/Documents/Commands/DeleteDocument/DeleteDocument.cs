using IdaraTech.Application.Common.Interfaces;

namespace IdaraTech.Application.Documents.Commands.DeleteDocument;

public record DeleteDocumentCommand(int Id) : IRequest;

public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteDocumentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Documents
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        _context.Documents.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
