using IdaraTech.Application.Common.Interfaces;
using IdaraTech.Application.Common.Models;
using IdaraTech.Application.Demandes.Queries.GetDemandById;
using Microsoft.EntityFrameworkCore;

namespace IdaraTech.Application.Demandes.Queries.GetDemandesWithPagination;

public record GetDemandesWithPaginationQuery : IRequest<PaginatedList<DemandeDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class GetDemandesWithPaginationQueryHandler : IRequestHandler<GetDemandesWithPaginationQuery, PaginatedList<DemandeDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDemandesWithPaginationQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<DemandeDto>> Handle(GetDemandesWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Demandes
            .AsNoTracking()
            .Include(d => d.Citoyen)
            .Include(d => d.ServiceConcerne)
            .Include(d => d.AgentTraitant)
            .Include(d => d.Documents)
            .OrderByDescending(d => d.Id)
            .Select(d => new DemandeDto
            {
                Id = d.Id,
                CitoyenId = d.CitoyenId,
                CitoyenNom = d.Citoyen != null ? d.Citoyen.Nom : string.Empty,
                Description = d.Description,
                Statut = (int)d.Statut,
                StatutName = d.Statut.ToString(),
                ServiceId = d.ServiceId,
                ServiceNom = d.ServiceConcerne != null ? d.ServiceConcerne.Nom : null,
                AgentId = d.AgentId,
                AgentNom = d.AgentTraitant != null ? d.AgentTraitant.Nom : null,
                Documents = d.Documents.Select(doc => new DocumentDto
                {
                    Id = doc.Id,
                    Nom = doc.Nom,
                    Type = doc.Type
                }).ToList()
            })
            .AsQueryable();

        return await PaginatedList<DemandeDto>.CreateAsync(
            query,
            request.PageNumber < 1 ? 1 : request.PageNumber,
            request.PageSize < 1 ? 10 : request.PageSize,
            cancellationToken);
    }
}
