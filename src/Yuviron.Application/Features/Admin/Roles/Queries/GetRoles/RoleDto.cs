using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Roles.Queries.GetRoles;

public sealed record RoleDto(
    Guid Id, 
    string Name, 
    int UserCount,
    List<AppPermission> Permissions 
);