using System;
using Yuviron.Domain.Common;
using Yuviron.Domain.Enums; // <-- Подключили енамку

namespace Yuviron.Domain.Entities;

public sealed class CopyrightClaim : Entity 
{
    public CopyrightEntityType EntityType { get; private set; }
    public Guid EntityId { get; private set; } 
    public Guid OwnerArtistId { get; private set; }
    public bool OwnsAllRights { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Artist OwnerArtist { get; private set; } = null!;

    private CopyrightClaim() { }

    public static CopyrightClaim Create(
        CopyrightEntityType entityType, Guid entityId, Guid artistId, 
        bool ownsAllRights, string? notes, DateTime utcNow)
    {
        return new CopyrightClaim
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            OwnerArtistId = artistId,
            OwnsAllRights = ownsAllRights,
            Notes = notes?.Trim(),
            CreatedAt = utcNow
        };
    }
}