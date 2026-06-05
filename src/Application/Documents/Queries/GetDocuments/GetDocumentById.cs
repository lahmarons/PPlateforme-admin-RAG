using IdaraTech.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdaraTech.Application.Documents.Queries.GetDocuments;

public record GetDocumentByIdQuery : IRequest<DocumentDto>
{
    public int Id { get; init; }
}

public class GetDocumentByIdQueryHandler : IRequestHandler<GetDocumentByIdQuery, DocumentDto>
{
    private readonly IApplicationDbContext _context;

    public GetDocumentByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DocumentDto> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            throw new IdaraTech.Application.Common.Exceptions.NotFoundException(nameof(Domain.Entities.Document), request.Id);
        }

        return new DocumentDto
        {
            Id = entity.Id,
            Nom = entity.Nom,
            Type = entity.Type,
            Contenu = entity.Contenu,
            DemandeId = entity.DemandeId
        };
    }
}
