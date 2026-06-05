using IdaraTech.Application.Common.Interfaces;
using IdaraTech.Domain.Entities;
using IdaraTech.Application.Common.Exceptions;
using IdaraTech.Domain.Constants;
using FluentValidation.Results;

namespace IdaraTech.Application.Agents.Commands.CreateAgent;

public record CreateAgentCommand : IRequest<int>
{
    public string Nom { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string MotDePasse { get; init; } = string.Empty;
    public string Matricule { get; init; } = string.Empty;
    public string Grade { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
    public int ServiceId { get; init; }
}

public class CreateAgentCommandHandler : IRequestHandler<CreateAgentCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;

    public CreateAgentCommandHandler(IApplicationDbContext context, IIdentityService identityService)
    {
        _context = context;
        _identityService = identityService;
    }

    public async Task<int> Handle(CreateAgentCommand request, CancellationToken cancellationToken)
    {
        // 1. Create the ASP.NET Core Identity user
        var (createUserResult, userId) = await _identityService.CreateUserAsync(request.Email, request.MotDePasse);

        if (!createUserResult.Succeeded)
        {
            var failures = createUserResult.Errors.Select(error => new ValidationFailure("Email", error));
            throw new IdaraTech.Application.Common.Exceptions.ValidationException(failures);
        }

        // 2. Assign the Agent role to the user
        var roleResult = await _identityService.AddToRoleAsync(userId, Roles.Agent);

        if (!roleResult.Succeeded)
        {
            // Roll back user creation on failure
            await _identityService.DeleteUserAsync(userId);
            var failures = roleResult.Errors.Select(error => new ValidationFailure("Role", error));
            throw new IdaraTech.Application.Common.Exceptions.ValidationException(failures);
        }

        // 3. Create the Agent domain entity
        var entity = new Agent
        {
            Nom = request.Nom,
            Email = request.Email,
            MotDePasse = request.MotDePasse,
            Matricule = request.Matricule,
            Grade = request.Grade,
            IsActive = request.IsActive,
            ServiceId = request.ServiceId
        };

        _context.Agents.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
