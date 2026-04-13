using Yuviron.Domain.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Domain.Entities;

public class ArtistTeamMember : Entity
{
    public Guid ArtistId { get; private set; }
    public Guid UserId { get; private set; }
    public ArtistTeamRole Role { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public virtual Artist Artist { get; private set; } = null!;
    public virtual User User { get; private set; } = null!;

    private ArtistTeamMember() { }

    public static ArtistTeamMember Create(Guid artistId, Guid userId, ArtistTeamRole role, DateTime utcNow)
    {
        return new ArtistTeamMember
        {
            Id = Guid.NewGuid(),
            ArtistId = artistId,
            UserId = userId,
            Role = role,
            CreatedAt = utcNow
        };
    }

    public void ChangeRole(ArtistTeamRole newRole)
    {
        Role = newRole;
    }
}