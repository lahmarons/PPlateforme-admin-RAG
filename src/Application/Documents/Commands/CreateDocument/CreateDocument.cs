using IdaraTech.Application.Common.Interfaces;
using IdaraTech.Domain.Entities;

namespace IdaraTech.Application.Documents.Commands.CreateDocument;

public record CreateDocumentCommand : IRequest<int>
{
    public string Nom { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public int DemandeId { get; init; }
}

public class CreateDocumentCommandHandler : IRequestHandler<CreateDocumentCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateDocumentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
    {
        var entity = new Document
        {
            Nom = request.Nom,
            Type = request.Type,
            DemandeId = request.DemandeId
        };

        _context.Documents.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
