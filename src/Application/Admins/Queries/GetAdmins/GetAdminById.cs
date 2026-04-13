using IdaraTech.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdaraTech.Application.Admins.Queries.GetAdmins;

public record GetAdminByIdQuery : IRequest<AdminDto>
{
    public int Id { get; init; }
}

public class GetAdminByIdQueryHandler : IRequestHandler<GetAdminByIdQuery, AdminDto>
{
    private readonly IApplicationDbContext _context;

    public GetAdminByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminDto> Handle(GetAdminByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Admins
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            throw new IdaraTech.Application.Common.Exceptions.NotFoundException(nameof(Domain.Entities.Admin), request.Id);
        }

        return new AdminDto
        {
            Id = entity.Id,
            Nom = entity.Nom,
            Email = entity.Email
        };
    }
}
