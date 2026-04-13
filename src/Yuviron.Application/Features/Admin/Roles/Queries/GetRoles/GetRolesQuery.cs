using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Roles.Queries.GetRoles;

public sealed record GetRolesQuery : IRequest<List<RoleDto>>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessAdminPanel; 
}