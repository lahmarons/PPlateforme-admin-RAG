using IdaraTech.Application.Common.Interfaces;
using IdaraTech.Application.Services.Queries.GetServices;
using Microsoft.EntityFrameworkCore;

namespace IdaraTech.Application.Services.Queries.GetServicesList;

public record GetServicesQuery : IRequest<List<ServiceDto>>;

public class GetServicesQueryHandler : IRequestHandler<GetServicesQuery, List<ServiceDto>>
{
    private readonly IApplicationDbContext _context;

    public GetServicesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ServiceDto>> Handle(GetServicesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Services
            .Include(s => s.Prestataire)
            .Select(s => new ServiceDto
            {
                Id = s.Id,
                Nom = s.Nom,
                Description = s.Description,
                IsActive = s.IsActive,
                PrestataireId = s.PrestataireId,
                PrestataireNom = s.Prestataire != null ? s.Prestataire.Nom : string.Empty
            })
            .ToListAsync(cancellationToken);
    }
}
