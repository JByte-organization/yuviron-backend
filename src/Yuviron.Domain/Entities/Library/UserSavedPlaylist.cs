namespace Yuviron.Domain.Entities;

public class UserSavedPlaylist
{
    public Guid UserId { get; private set; }
    public Guid PlaylistId { get; private set; }
    public DateTime SavedAt { get; private set; }

    public virtual User User { get; private set; } = null!;
    public virtual Playlist Playlist { get; private set; } = null!;

    private UserSavedPlaylist() { }

    public UserSavedPlaylist(Guid userId, Guid playlistId, DateTime utcNow)
    {
        UserId = userId;
        PlaylistId = playlistId;
        SavedAt = utcNow;
    }
}