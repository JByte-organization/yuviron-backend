using System;
using System.Collections.Generic;
using System.Text;

namespace Yuviron.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; set; }
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        if (obj is Entity other)
            return Id.Equals(other.Id);

        return false;
    }

    public override int GetHashCode() => Id.GetHashCode();
}