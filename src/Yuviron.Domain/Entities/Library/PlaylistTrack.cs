using System;
using System.Collections.Generic;
using System.Text;
namespace Yuviron.Domain.Entities;

public class PlaylistTrack
{
    public Guid PlaylistId { get; set; }
    public virtual Playlist Playlist { get; set; } = null!;

    public Guid TrackId { get; set; }
    public virtual Track Track { get; set; } = null!;

    public int Position { get; set; }
    public DateTime AddedAt { get; set; }
    public Guid AddedByUserId { get; set; }
}
