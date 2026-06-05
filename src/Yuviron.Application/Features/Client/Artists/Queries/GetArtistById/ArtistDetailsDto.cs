using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistById;

public sealed record ClientSocialLinkDto(
    string Type, 
    string Url
);

public sealed record ArtistDetailsDto(
    Guid Id,
    string Name,
    string? Bio,
    string? AvatarUrl,
    string? BannerUrl,
    VerificationStatus VerificationStatus,
    int ListenersCount,
    List<ClientSocialLinkDto> SocialLinks, 
    bool IsFollowed = false
);
