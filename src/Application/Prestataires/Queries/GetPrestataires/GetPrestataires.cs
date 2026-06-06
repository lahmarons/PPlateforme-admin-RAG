using IdaraTech.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdaraTech.Application.Prestataires.Queries.GetPrestataires;

public record GetPrestatairesQuery : IRequest<List<PrestataireDto>>
{
    public int? PrestataireId { get; init; }
    public string? Recherche { get; init; }
}

public class GetPrestatairesQueryHandler : IRequestHandler<GetPrestatairesQuery, List<PrestataireDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPrestatairesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PrestataireDto>> Handle(GetPrestatairesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Prestataires.AsNoTracking();

        if (request.PrestataireId.HasValue)
        {
            query = query.Where(p => p.Id == request.PrestataireId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Recherche))
        {
            var term = request.Recherche.Trim();
            query = query.Where(p => p.Nom.Contains(term) || p.Description.Contains(term));
        }

        return await query
            .OrderBy(p => p.Nom)
            .Select(p => new PrestataireDto
            {
                Id = p.Id,
                Nom = p.Nom,
                Description = p.Description,
                Logo = p.Logo
            })
            .ToListAsync(cancellationToken);
    }
}
