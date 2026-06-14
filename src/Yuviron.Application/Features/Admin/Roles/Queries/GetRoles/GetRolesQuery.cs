using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Roles.Queries.GetRoles;

public sealed record GetRolesQuery(AppPermission? RequiredPermission = null) : IRequest<List<RoleDto>>, ISecuredRequest
{
    AppPermission ISecuredRequest.RequiredPermission => AppPermission.AccessAdminPanel; 
}
