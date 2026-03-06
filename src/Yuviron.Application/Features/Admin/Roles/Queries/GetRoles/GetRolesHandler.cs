using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;

namespace Yuviron.Application.Features.Admin.Roles.Queries.GetRoles;

public sealed class GetRolesHandler : IRequestHandler<GetRolesQuery, List<RoleDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRolesHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Roles
            .AsNoTracking()
            .Select(r => new RoleDto(
                r.Id, 
                r.Name, 
                r.UserRoles.Count 
            ))
            .ToListAsync(cancellationToken);
    }
}