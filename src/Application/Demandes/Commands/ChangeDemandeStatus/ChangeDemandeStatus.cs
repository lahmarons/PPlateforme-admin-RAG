using IdaraTech.Application.Common.Interfaces;
using IdaraTech.Application.Common.Exceptions;
using IdaraTech.Domain.Entities;
using IdaraTech.Domain.Events;
using IdaraTech.Domain.Enums;
using IdaraTech.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace IdaraTech.Application.Demandes.Commands.ChangeDemandeStatus;

public record ChangeDemandeStatusCommand : IRequest
{
    public int DemandeId { get; init; }

    public Statut NewStatus { get; init; }
}

public class ChangeDemandeStatusCommandHandler : IRequestHandler<ChangeDemandeStatusCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public ChangeDemandeStatusCommandHandler(IApplicationDbContext context, IUser user, IIdentityService identityService)
    {
        _context = context;
        _user = user;
        _identityService = identityService;
    }

    public async Task Handle(ChangeDemandeStatusCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Demandes
            .FindAsync(new object[] { request.DemandeId }, cancellationToken: cancellationToken);

        if (entity is null)
        {
            throw new IdaraTech.Application.Common.Exceptions.NotFoundException(nameof(Demande), request.DemandeId);
        }

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

        var previousStatus = entity.Statut;
        entity.ChangerStatut(request.NewStatus);

        entity.AddDomainEvent(new DemandeStatusChangedEvent(entity, previousStatus));

        await _context.SaveChangesAsync(cancellationToken);
    }
}
