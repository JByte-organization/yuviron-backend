using System;
using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Domain.Entities;

public class ExternalMapping : Entity
{
    public Guid InternalId { get; private set; }
    
    public string EntityType { get; private set; } = null!;
    
    public ExternalProvider Provider { get; private set; }
    
    public string ExternalId { get; private set; } = null!; 

    private ExternalMapping() { } 

    public static ExternalMapping Create(Guid internalId, string entityType, ExternalProvider provider, string externalId)
    {
        return new ExternalMapping
        {
            InternalId = internalId,
            EntityType = entityType,
            Provider = provider,
            ExternalId = externalId
        };
    }
}