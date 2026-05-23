using System;

namespace Yuviron.Application.Features.Client.Library.Queries.GetUserFollowed;

public sealed record FollowedProfileDto(
    Guid Id,
    string Name,
    string? AvatarUrl,
    string Type,
    DateTime FollowedAt
);