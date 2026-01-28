using System;
using System.Collections.Generic;
using System.Text;
namespace Yuviron.Domain.Entities;

public class UserFollowArtist
{
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public Guid ArtistId { get; set; }
    public virtual Artist Artist { get; set; } = null!;

    public DateTime FollowedAt { get; set; }
    public bool NotifyNewReleases { get; set; }
}
