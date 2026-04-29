using IdaraTech.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdaraTech.Application.Prestataires.Queries.GetPrestataires;

public record GetPrestataireByIdQuery : IRequest<PrestataireDto>
{
    public int Id { get; init; }
}

public class GetPrestataireByIdQueryHandler : IRequestHandler<GetPrestataireByIdQuery, PrestataireDto>
{
    private readonly IApplicationDbContext _context;

    public GetPrestataireByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PrestataireDto> Handle(GetPrestataireByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Prestataires
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            throw new IdaraTech.Application.Common.Exceptions.NotFoundException(nameof(Domain.Entities.Prestataire), request.Id);
        }

        return new PrestataireDto
        {
            Id = entity.Id,
            Nom = entity.Nom,
            Description = entity.Description
        };
    }
}
