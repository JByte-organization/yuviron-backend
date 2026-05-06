using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistById;

public sealed record ArtistDetailsDto(
    Guid Id,
    string Name,
    string? Bio,
    string? AvatarUrl,
    string? BannerUrl,
    VerificationStatus VerificationStatus,
    int ListenersCount
);
