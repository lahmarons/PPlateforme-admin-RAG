using IdaraTech.Application.Admins.Queries.GetAdmins;
using IdaraTech.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IdaraTech.Application.Admins.Queries.GetAdminsList;

public record GetAdminsQuery : IRequest<List<AdminDto>>;

public class GetAdminsQueryHandler : IRequestHandler<GetAdminsQuery, List<AdminDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAdminsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AdminDto>> Handle(GetAdminsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Admins
            .Select(a => new AdminDto
            {
                Id = a.Id,
                Nom = a.Nom,
                Email = a.Email
            })
            .ToListAsync(cancellationToken);
    }
}
