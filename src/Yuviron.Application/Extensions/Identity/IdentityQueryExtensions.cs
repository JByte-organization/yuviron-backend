using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using System.Linq;

namespace Yuviron.Application.Extensions;

public static class IdentityQueryExtensions
{
    public static IQueryable<User> WhereHasPermission(this IQueryable<User> query, AppPermission? permission)
    {
        if (!permission.HasValue) return query;
        
        var permissionName = permission.Value.ToString();
        return query.Where(u => u.UserRoles.Any(ur => 
            ur.Role.RolePermissions.Any(rp => rp.Permission.Name == permissionName)));
    }

    public static IQueryable<Role> WhereHasPermission(this IQueryable<Role> query, AppPermission? permission)
    {
        if (!permission.HasValue) return query;

        var permissionName = permission.Value.ToString();
        return query.Where(r => r.RolePermissions.Any(rp => rp.Permission.Name == permissionName));
    }

    public static IQueryable<ArtistTeamMember> WhereHasPermission(this IQueryable<ArtistTeamMember> query, AppPermission? permission)
    {
        if (!permission.HasValue) return query;

        var permissionName = permission.Value.ToString();
        return query.Where(tm => tm.User.UserRoles.Any(ur => 
            ur.Role.RolePermissions.Any(rp => rp.Permission.Name == permissionName)));
    }
}
