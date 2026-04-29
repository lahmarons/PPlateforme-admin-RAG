using IdaraTech.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdaraTech.Application.Services.Queries.GetServices;

public record GetServiceByIdQuery : IRequest<ServiceDto>
{
    public int Id { get; init; }
}

public class GetServiceByIdQueryHandler : IRequestHandler<GetServiceByIdQuery, ServiceDto>
{
    private readonly IApplicationDbContext _context;

    public GetServiceByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceDto> Handle(GetServiceByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Services
            .AsNoTracking()
            .Include(s => s.Prestataire)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            throw new IdaraTech.Application.Common.Exceptions.NotFoundException(nameof(Domain.Entities.Service), request.Id);
        }

        return new ServiceDto
        {
            Id = entity.Id,
            Nom = entity.Nom,
            Description = entity.Description,
            IsActive = entity.IsActive,
            PrestataireId = entity.PrestataireId,
            PrestataireNom = entity.Prestataire != null ? entity.Prestataire.Nom : string.Empty
        };
    }
}
