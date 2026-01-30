using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

// Для полиморфизма (EntityId может быть TrackId или AlbumId)
public enum ArtistPinType { Track = 1, Album = 2 }

public class ArtistPin : Entity
{
    public Guid ArtistId { get; private set; }
    public ArtistPinType EntityType { get; private set; }
    public Guid EntityId { get; private set; } // ID трека или альбома
    public int Position { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public virtual Artist Artist { get; private set; } = null!;

    private ArtistPin() { }
}
