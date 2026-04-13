using IdaraTech.Application.Common.Interfaces;
using IdaraTech.Application.Common.Exceptions;
using IdaraTech.Domain.Entities;

namespace IdaraTech.Application.Demandes.Queries.GetDemandById;

public record GetDemandeByIdQuery : IRequest<DemandeDto>
{
    public int Id { get; init; }
}

public class GetDemandeByIdQueryHandler : IRequestHandler<GetDemandeByIdQuery, DemandeDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDemandeByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<DemandeDto> Handle(GetDemandeByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Demandes
            .Include(x => x.Citoyen)
            .Include(x => x.ServiceConcerne)
            .Include(x => x.AgentTraitant)
            .Include(x => x.Documents)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            throw new IdaraTech.Application.Common.Exceptions.NotFoundException(nameof(Demande), request.Id);
        }

        return _mapper.Map<DemandeDto>(entity);
    }
}
