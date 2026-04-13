using IdaraTech.Application.Common.Interfaces;

namespace IdaraTech.Application.Agents.Commands.UpdateAgent;

public record UpdateAgentCommand : IRequest
{
    public int Id { get; init; }
    public string Nom { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string MotDePasse { get; init; } = string.Empty;
    public string Matricule { get; init; } = string.Empty;
    public string Grade { get; init; } = string.Empty;
    public int? ServiceId { get; init; }
}

public class UpdateAgentCommandHandler : IRequestHandler<UpdateAgentCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateAgentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateAgentCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Agents
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Nom = request.Nom;
        entity.Email = request.Email;
        entity.MotDePasse = request.MotDePasse;
        entity.Matricule = request.Matricule;
        entity.Grade = request.Grade;
        entity.ServiceId = request.ServiceId;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
