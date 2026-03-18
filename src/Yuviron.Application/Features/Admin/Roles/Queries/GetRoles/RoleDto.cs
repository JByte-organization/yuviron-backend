using System;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Roles.Queries.GetRoles;

// Заменяем List<string> на List<AppPermission>
public sealed record RoleDto(
    Guid Id, 
    string Name, 
    int UserCount,
    List<AppPermission> Permissions 
);