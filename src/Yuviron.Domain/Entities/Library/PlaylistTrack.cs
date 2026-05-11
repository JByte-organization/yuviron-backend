namespace Yuviron.Domain.Entities;

public class PlaylistTrack
{
    public Guid PlaylistId { get; private set; }
    public Guid TrackId { get; private set; }
    public double Position { get; private set; } 
    public DateTime AddedAt { get; private set; }
    public Guid AddedByUserId { get; private set; }

    public virtual Playlist Playlist { get; private set; } = null!;
    public virtual Track Track { get; private set; } = null!;

    private PlaylistTrack() { }

    public PlaylistTrack(Guid playlistId, Guid trackId, double position, Guid addedBy, DateTime utcNow)
    {
        PlaylistId = playlistId;
        TrackId = trackId;
        Position = position;
        AddedByUserId = addedBy;
        AddedAt = utcNow;
    }

    public void UpdatePosition(double newPosition) => Position = newPosition;
}