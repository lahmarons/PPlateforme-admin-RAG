using IdaraTech.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdaraTech.Application.Citoyens.Queries.GetCitoyenById;

public record GetCitoyenByIdQuery : IRequest<CitoyenDto>
{
    public int Id { get; init; }
}

public class GetCitoyenByIdQueryHandler : IRequestHandler<GetCitoyenByIdQuery, CitoyenDto>
{
    private readonly IApplicationDbContext _context;

    public GetCitoyenByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CitoyenDto> Handle(GetCitoyenByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Citoyens
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            throw new IdaraTech.Application.Common.Exceptions.NotFoundException(nameof(Domain.Entities.Citoyen), request.Id);
        }

        return new CitoyenDto
        {
            Id = entity.Id,
            Nom = entity.Nom,
            Email = entity.Email,
            Adresse = entity.Adresse,
            Telephone = entity.Telephone
        };
    }
}
