using IdaraTech.Application.Common.Interfaces;
using IdaraTech.Application.Common.Models;
using IdaraTech.Application.Demandes.Queries.GetDemandById;
using IdaraTech.Domain.Constants;
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
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public GetDemandesWithPaginationQueryHandler(IApplicationDbContext context, IUser user, IIdentityService identityService)
    {
        _context = context;
        _user = user;
        _identityService = identityService;
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
            .AsQueryable();

        if (_user.Id != null && _user.Roles != null && _user.Roles.Contains(Roles.Citoyen))
        {
            var email = await _identityService.GetUserNameAsync(_user.Id);
            if (!string.IsNullOrEmpty(email))
            {
                var citoyen = await _context.Citoyens.FirstOrDefaultAsync(c => c.Email == email, cancellationToken);
                if (citoyen != null)
                {
                    query = query.Where(d => d.CitoyenId == citoyen.Id);
                }
                else
                {
                    query = query.Where(d => false);
                }
            }
            else
            {
                query = query.Where(d => false);
            }
        }
        else if (_user.Id != null && _user.Roles != null && _user.Roles.Contains(Roles.Agent))
        {
            var email = await _identityService.GetUserNameAsync(_user.Id);
            if (!string.IsNullOrEmpty(email))
            {
                var agent = await _context.Agents.FirstOrDefaultAsync(a => a.Email == email, cancellationToken);
                if (agent != null)
                {
                    query = query.Where(d => d.ServiceId == agent.ServiceId);
                }
                else
                {
                    query = query.Where(d => false);
                }
            }
            else
            {
                query = query.Where(d => false);
            }
        }

        var selectQuery = query.Select(d => new DemandeDto
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
        });

        return await PaginatedList<DemandeDto>.CreateAsync(
            selectQuery,
            request.PageNumber < 1 ? 1 : request.PageNumber,
            request.PageSize < 1 ? 10 : request.PageSize,
            cancellationToken);
    }
}
