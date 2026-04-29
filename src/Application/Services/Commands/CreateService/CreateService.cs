using IdaraTech.Application.Common.Interfaces;
using IdaraTech.Domain.Entities;

namespace IdaraTech.Application.Services.Commands.CreateService;

public record CreateServiceCommand : IRequest<int>
{
    public string Nom { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool IsActive { get; init; } = true;
    public int? PrestataireId { get; init; }
}

public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateServiceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        var entity = new Service
        {
            Nom = request.Nom,
            Description = request.Description,
            IsActive = request.IsActive,
            PrestataireId = request.PrestataireId
        };

        _context.Services.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
