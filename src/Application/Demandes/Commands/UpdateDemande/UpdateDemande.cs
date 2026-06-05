using IdaraTech.Application.Common.Interfaces;
using IdaraTech.Application.Common.Exceptions;
using IdaraTech.Domain.Enums;
using IdaraTech.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace IdaraTech.Application.Demandes.Commands.UpdateDemande;

public record UpdateDemandeCommand : IRequest
{
    public int Id { get; init; }
    public string Description { get; init; } = string.Empty;
    public int? ServiceId { get; init; }
    public int? AgentId { get; init; }
    public Statut Statut { get; init; }
}

public class UpdateDemandeCommandHandler : IRequestHandler<UpdateDemandeCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public UpdateDemandeCommandHandler(IApplicationDbContext context, IUser user, IIdentityService identityService)
    {
        _context = context;
        _user = user;
        _identityService = identityService;
    }

    public async Task Handle(UpdateDemandeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Demandes
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        if (_user.Id != null && _user.Roles != null && _user.Roles.Contains(Roles.Agent))
        {
            var email = await _identityService.GetUserNameAsync(_user.Id);
            if (!string.IsNullOrEmpty(email))
            {
                var agent = await _context.Agents.FirstOrDefaultAsync(a => a.Email == email, cancellationToken);
                if (agent == null || entity.ServiceId != agent.ServiceId)
                {
                    throw new ForbiddenAccessException();
                }
            }
            else
            {
                throw new ForbiddenAccessException();
            }
        }

        entity.Description = request.Description;
        entity.ServiceId = request.ServiceId;
        entity.AgentId = request.AgentId;
        entity.Statut = request.Statut;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
