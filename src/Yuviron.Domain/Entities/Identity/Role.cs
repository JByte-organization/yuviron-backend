using Yuviron.Domain.Common;
using Yuviron.Domain.Entities;

public class Role : Entity
{
    public string Name { get; private set; } = string.Empty;

    public virtual ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    public virtual ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    private Role() { }

    public static Role Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Role name is required");
        return new Role { Id = Guid.NewGuid(), Name = name.Trim() };
    }

    public void SyncPermissions(IEnumerable<Guid> permissionIds)
    {
        var newIds = permissionIds.Distinct().ToList();
        
        var toRemove = RolePermissions.Where(rp => !newIds.Contains(rp.PermissionId)).ToList();
        foreach (var item in toRemove) RolePermissions.Remove(item);

        var currentIds = RolePermissions.Select(rp => rp.PermissionId).ToList();
        foreach (var id in newIds.Where(id => !currentIds.Contains(id)))
        {
            RolePermissions.Add(RolePermission.Create(Id, id));
        }
    }
}