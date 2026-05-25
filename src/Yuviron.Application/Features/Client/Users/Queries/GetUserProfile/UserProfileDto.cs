using System;

namespace Yuviron.Application.Features.Client.Users.Queries.GetUserProfile;

public sealed record UserProfileDto(
    Guid Id,
    string Name,
    string? AvatarUrl,
    string? BannerUrl,
    string? Bio,
    int FollowersCount,
    int FollowingCount,
    int PublicPlaylistsCount,
    bool IsFollowedByCurrentUser
);