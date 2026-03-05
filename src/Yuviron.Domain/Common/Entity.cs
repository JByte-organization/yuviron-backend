using System;
using System.Collections.Generic;
using MediatR;

namespace Yuviron.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; set; }

    private readonly List<INotification> _domainEvents = new();

    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(INotification domainEvent)
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