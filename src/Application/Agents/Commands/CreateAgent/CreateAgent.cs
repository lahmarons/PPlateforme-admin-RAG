using IdaraTech.Application.Common.Interfaces;
using IdaraTech.Domain.Entities;

namespace IdaraTech.Application.Agents.Commands.CreateAgent;

public record CreateAgentCommand : IRequest<int>
{
    public string Nom { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string MotDePasse { get; init; } = string.Empty;
    public string Matricule { get; init; } = string.Empty;
    public string Grade { get; init; } = string.Empty;
    public int ServiceId { get; init; }
}

public class CreateAgentCommandHandler : IRequestHandler<CreateAgentCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateAgentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateAgentCommand request, CancellationToken cancellationToken)
    {
        var entity = new Agent
        {
            Nom = request.Nom,
            Email = request.Email,
            MotDePasse = request.MotDePasse,
            Matricule = request.Matricule,
            Grade = request.Grade,
            ServiceId = request.ServiceId
        };

        _context.Agents.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
