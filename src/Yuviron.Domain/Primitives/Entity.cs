using System;
using System.Collections.Generic;

namespace Yuviron.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; set; }

    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

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