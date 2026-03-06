using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Artists.Queries.DTOs;

public record ArtistDetailsDto(
    Guid Id,
    Guid? OwnerUserId, // Если артист привязан к аккаунту юзера
    string Name,
    string? Bio,
    string? AvatarUrl,
    string? BannerUrl,
    bool IsVerified,
    VerificationStatus VerificationStatus,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime UpdatedAt
);