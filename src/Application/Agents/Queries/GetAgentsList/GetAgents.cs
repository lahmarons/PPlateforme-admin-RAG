using IdaraTech.Application.Common.Interfaces;
using IdaraTech.Application.Agents.Queries.GetAgents;
using Microsoft.EntityFrameworkCore;

namespace IdaraTech.Application.Agents.Queries.GetAgentsList;

public record GetAgentsQuery : IRequest<List<AgentDto>>;

public class GetAgentsQueryHandler : IRequestHandler<GetAgentsQuery, List<AgentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAgentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AgentDto>> Handle(GetAgentsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Agents
            .Include(a => a.Service)
            .Select(a => new AgentDto
            {
                Id = a.Id,
                Nom = a.Nom,
                Email = a.Email,
                Matricule = a.Matricule,
                Grade = a.Grade,
                ServiceId = a.ServiceId ?? 0,
                ServiceNom = a.Service != null ? a.Service.Nom : string.Empty
            })
            .ToListAsync(cancellationToken);
    }
}
