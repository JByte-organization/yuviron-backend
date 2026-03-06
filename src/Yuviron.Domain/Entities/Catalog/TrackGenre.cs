using System;

namespace Yuviron.Domain.Entities;

public class TrackGenre
{
    public Guid TrackId { get; private set; }
    public virtual Track Track { get; private set; } = null!;

    public Guid GenreId { get; private set; }
    public virtual Genre Genre { get; private set; } = null!;

    private TrackGenre() { } 

    public TrackGenre(Guid trackId, Guid genreId)
    {
        TrackId = trackId;
        GenreId = genreId;
    }
}