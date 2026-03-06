using Yuviron.Domain.Entities;
namespace Yuviron.Domain.Entities;
public class UserFollowArtist
{
    public Guid UserId { get; private set; }
    public Guid ArtistId { get; private set; }
    public DateTime FollowedAt { get; private set; }
    public bool NotifyNewReleases { get; private set; }

    public virtual User User { get; private set; } = null!;
    public virtual Artist Artist { get; private set; } = null!;

    private UserFollowArtist() { }

    public static UserFollowArtist Create(Guid userId, Guid artistId, bool notify, DateTime utcNow)
    {
        return new UserFollowArtist
        {
            UserId = userId,
            ArtistId = artistId,
            NotifyNewReleases = notify,
            FollowedAt = utcNow
        };
    }
}