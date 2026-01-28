using System;
using System.Collections.Generic;
using System.Text;
namespace Yuviron.Domain.Entities;

public class TrackArtist
{
    public Guid TrackId { get; set; }
    public virtual Track Track { get; set; } = null!;

    public Guid ArtistId { get; set; }
    public virtual Artist Artist { get; set; } = null!;

    public ArtistRole Role { get; set; }
}
