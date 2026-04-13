using IdaraTech.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdaraTech.Application.Agents.Queries.GetAgents;

public record GetAgentByIdQuery : IRequest<AgentDto>
{
    public int Id { get; init; }
}

public class GetAgentByIdQueryHandler : IRequestHandler<GetAgentByIdQuery, AgentDto>
{
    private readonly IApplicationDbContext _context;

    public GetAgentByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AgentDto> Handle(GetAgentByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Agents
            .AsNoTracking()
            .Include(a => a.Service)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            throw new IdaraTech.Application.Common.Exceptions.NotFoundException(nameof(Domain.Entities.Agent), request.Id);
        }

        return new AgentDto
        {
            Id = entity.Id,
            Nom = entity.Nom,
            Email = entity.Email,
            Matricule = entity.Matricule,
            Grade = entity.Grade,
            ServiceId = entity.ServiceId ?? 0,
            ServiceNom = entity.Service?.Nom ?? string.Empty
        };
    }
}
