using IdaraTech.Application.Common.Interfaces;

namespace IdaraTech.Application.Documents.Commands.UpdateDocument;

public record UpdateDocumentCommand : IRequest
{
    public int Id { get; init; }
    public string Nom { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public int DemandeId { get; init; }
}

public class UpdateDocumentCommandHandler : IRequestHandler<UpdateDocumentCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateDocumentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateDocumentCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Documents
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Nom = request.Nom;
        entity.Type = request.Type;
        entity.DemandeId = request.DemandeId;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
