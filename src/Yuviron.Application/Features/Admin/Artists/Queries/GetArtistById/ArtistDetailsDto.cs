using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Artists.Queries;

public record ArtistOwnerDto(
    Guid UserId, 
    string Email, 
    string FirstName
);

public record ArtistDetailsDto(
    Guid Id,
    ArtistOwnerDto? Owner,
    string Name,
    string? Bio,
    string? AvatarUrl,
    string? BannerUrl,
    VerificationStatus VerificationStatus,
    int TotalAlbums,   
    int TotalTracks,   
    DateTime CreatedAt,
    DateTime UpdatedAt
);