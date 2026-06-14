using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums; 

namespace Yuviron.Application.Features.Admin.Roles.Queries.GetRoles;

public sealed class GetRolesHandler : IRequestHandler<GetRolesQuery, List<RoleDto>>
{
    private readonly IIdentityContext _identityContext;

    public GetRolesHandler(IIdentityContext identityContext)
    {
        _identityContext = identityContext;
    }

    public async Task<List<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var rolesFromDb = await _identityContext.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name) 
            .Select(r => new 
            {
                r.Id, 
                r.Name, 
                UserCount = r.UserRoles.Count(),
                PermissionNames = r.RolePermissions.Select(rp => rp.Permission.Name).ToList() 
            })
            .ToListAsync(cancellationToken);

        var result = rolesFromDb.Select(r => new RoleDto(
            r.Id,
            r.Name,
            r.UserCount,
            r.PermissionNames
                .Select(name => Enum.TryParse<AppPermission>(name, out var perm) ? perm : (AppPermission?)null)
                .Where(p => p.HasValue)
                .Select(p => p!.Value) 
                .ToList()
        )).ToList();

        return result;
    }
}