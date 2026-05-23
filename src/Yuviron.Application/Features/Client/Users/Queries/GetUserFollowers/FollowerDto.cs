using System;

namespace Yuviron.Application.Features.Client.Users.Queries.GetUserFollowers;

public sealed record FollowerDto(
    Guid Id,
    string Name,
    string? AvatarUrl,
    DateTime FollowedAt
);