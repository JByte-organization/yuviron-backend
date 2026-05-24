using System;

namespace Yuviron.Domain.Entities;

public class UserFollowUser
{
    public Guid FollowerId { get; private set; } 
    public virtual User Follower { get; private set; } = null!;

    public Guid FolloweeId { get; private set; } 
    public virtual User Followee { get; private set; } = null!;

    public DateTime FollowedAt { get; private set; }

    private UserFollowUser() { }

    public UserFollowUser(Guid followerId, Guid followeeId, DateTime followedAt)
    {
        FollowerId = followerId;
        FolloweeId = followeeId;
        FollowedAt = followedAt;
    }
}