using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class Role : Entity
{
    public string Name { get; private set; } = string.Empty;

    public virtual ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    public virtual ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();
    private Role() { }

    public Role(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }
}
