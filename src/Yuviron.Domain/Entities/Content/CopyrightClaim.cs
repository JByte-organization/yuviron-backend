using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Common;

namespace Yuviron.Domain.Entities;

public enum CopyrightEntityType { Track = 1, Album = 2 }

public class CopyrightClaim : Entity
{
    public CopyrightEntityType EntityType { get; private set; }
    public Guid EntityId { get; private set; } // TrackId или AlbumId

    public Guid OwnerArtistId { get; private set; }
    public bool OwnsAllRights { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public virtual Artist OwnerArtist { get; private set; } = null!;

    private CopyrightClaim() { }
}
