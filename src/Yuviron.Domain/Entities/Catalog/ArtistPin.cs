using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public enum ArtistPinType { Track = 1, Album = 2 }

public class ArtistPin : Entity
{
    public Guid ArtistId { get; private set; }
    public ArtistPinType EntityType { get; private set; }
    public Guid EntityId { get; private set; }
    public int Position { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public virtual Artist Artist { get; private set; } = null!;

    private ArtistPin() { }

    public static ArtistPin Create(Guid artistId, ArtistPinType type, Guid entityId, int position, DateTime utcNow) // Добавили utcNow
    {
        return new ArtistPin
        {
            Id = Guid.NewGuid(),
            ArtistId = artistId,
            EntityType = type,
            EntityId = entityId,
            Position = position,
            CreatedAt = utcNow
        };
    }
}
