namespace Yuviron.Domain.Entities;

public class UserSavedAlbum
{
    public Guid UserId { get; private set; }
    public Guid AlbumId { get; private set; }
    public DateTime SavedAt { get; private set; }

    public virtual User User { get; private set; } = null!;
    public virtual Album Album { get; private set; } = null!;

    private UserSavedAlbum() { }

    public UserSavedAlbum(Guid userId, Guid albumId, DateTime utcNow)
    {
        UserId = userId;
        AlbumId = albumId;
        SavedAt = utcNow;
    }
}