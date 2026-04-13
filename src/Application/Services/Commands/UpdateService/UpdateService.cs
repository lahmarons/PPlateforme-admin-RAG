using IdaraTech.Application.Common.Interfaces;

namespace IdaraTech.Application.Services.Commands.UpdateService;

public record UpdateServiceCommand : IRequest
{
    public int Id { get; init; }
    public string Nom { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
}

public class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateServiceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Services
            .FindAsync(new object[] { request.Id }, cancellationToken);

        Guard.Against.NotFound(request.Id, entity);

        entity.Nom = request.Nom;
        entity.Description = request.Description;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
