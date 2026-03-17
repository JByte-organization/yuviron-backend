using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Artists.Queries;

public record ArtistOwnerDto(
    Guid UserId, 
    string Email, 
    string? DisplayName
);

public record ArtistDetailsDto(
    Guid Id,
    ArtistOwnerDto Owner,
    string Name,
    string? Bio,
    string? AvatarUrl,
    string? BannerUrl,
    VerificationStatus VerificationStatus,
    DateTime CreatedAt,
    DateTime UpdatedAt
); 