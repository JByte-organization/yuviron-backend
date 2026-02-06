using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public class Permission : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    private Permission() { }

    public static Permission Create(string name, string description)
    {
        return new Permission
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description
        };
    }
}