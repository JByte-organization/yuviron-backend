using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistSimilarArtists;

public sealed record SimilarArtistDto(
    Guid Id,
    string Name,
    string? AvatarUrl,
    VerificationStatus VerificationStatus,
    int FollowersCount
);
