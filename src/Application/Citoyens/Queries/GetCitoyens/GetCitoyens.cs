using IdaraTech.Application.Common.Interfaces;
using IdaraTech.Application.Citoyens.Queries.GetCitoyenById;
using Microsoft.EntityFrameworkCore;

namespace IdaraTech.Application.Citoyens.Queries.GetCitoyens;

public record GetCitoyensQuery : IRequest<List<CitoyenDto>>;

public class GetCitoyensQueryHandler : IRequestHandler<GetCitoyensQuery, List<CitoyenDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCitoyensQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CitoyenDto>> Handle(GetCitoyensQuery request, CancellationToken cancellationToken)
    {
        return await _context.Citoyens
            .Select(c => new CitoyenDto
            {
                Id = c.Id,
                Nom = c.Nom,
                Email = c.Email,
                Adresse = c.Adresse,
                Telephone = c.Telephone
            })
            .ToListAsync(cancellationToken);
    }
}
