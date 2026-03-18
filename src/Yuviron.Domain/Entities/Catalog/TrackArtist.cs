using System;
using System.Collections.Generic;
using System.Text;
using Yuviron.Domain.Enums;

namespace Yuviron.Domain.Entities;

public class TrackArtist
{
    public Guid TrackId { get; private set; }
    public virtual Track Track { get; private set; } = null!;

    public Guid ArtistId { get; private set; }
    public virtual Artist Artist { get; private set; } = null!;

    public ArtistRole Role { get; private set; } 

    private TrackArtist() { }

    public TrackArtist(Guid trackId, Guid artistId, ArtistRole role)
    {
        TrackId = trackId;
        ArtistId = artistId;
        Role = role;
    }
}