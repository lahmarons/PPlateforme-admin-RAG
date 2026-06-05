using IdaraTech.Application.Common.Interfaces;
using IdaraTech.Application.Common.Exceptions;
using IdaraTech.Domain.Entities;
using IdaraTech.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace IdaraTech.Application.Demandes.Queries.GetDemandById;

public record GetDemandeByIdQuery : IRequest<DemandeDto>
{
    public int Id { get; init; }
}

public class GetDemandeByIdQueryHandler : IRequestHandler<GetDemandeByIdQuery, DemandeDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public GetDemandeByIdQueryHandler(IApplicationDbContext context, IMapper mapper, IUser user, IIdentityService identityService)
    {
        _context = context;
        _mapper = mapper;
        _user = user;
        _identityService = identityService;
    }

    public async Task<DemandeDto> Handle(GetDemandeByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Demandes
            .Include(x => x.Citoyen)
            .Include(x => x.ServiceConcerne)
            .Include(x => x.AgentTraitant)
            .Include(x => x.Documents)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            throw new IdaraTech.Application.Common.Exceptions.NotFoundException(nameof(Demande), request.Id);
        }

        if (_user.Id != null && _user.Roles != null)
        {
            if (_user.Roles.Contains(Roles.Agent))
            {
                var email = await _identityService.GetUserNameAsync(_user.Id);
                var agent = await _context.Agents.FirstOrDefaultAsync(a => a.Email == email, cancellationToken);
                if (agent == null || entity.ServiceId != agent.ServiceId)
                {
                    throw new ForbiddenAccessException();
                }
            }
            else if (_user.Roles.Contains(Roles.Citoyen))
            {
                var email = await _identityService.GetUserNameAsync(_user.Id);
                var citoyen = await _context.Citoyens.FirstOrDefaultAsync(c => c.Email == email, cancellationToken);
                if (citoyen == null || entity.CitoyenId != citoyen.Id)
                {
                    throw new ForbiddenAccessException();
                }
            }
        }

        return _mapper.Map<DemandeDto>(entity);
    }
}
