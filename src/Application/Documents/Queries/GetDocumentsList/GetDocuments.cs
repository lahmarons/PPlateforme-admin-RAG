using IdaraTech.Application.Common.Interfaces;
using IdaraTech.Application.Documents.Queries.GetDocuments;
using Microsoft.EntityFrameworkCore;

namespace IdaraTech.Application.Documents.Queries.GetDocumentsList;

public record GetDocumentsQuery : IRequest<List<DocumentDto>>;

public class GetDocumentsQueryHandler : IRequestHandler<GetDocumentsQuery, List<DocumentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDocumentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DocumentDto>> Handle(GetDocumentsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Documents
            .Select(d => new DocumentDto
            {
                Id = d.Id,
                Nom = d.Nom,
                Type = d.Type,
                DemandeId = d.DemandeId
            })
            .ToListAsync(cancellationToken);
    }
}
