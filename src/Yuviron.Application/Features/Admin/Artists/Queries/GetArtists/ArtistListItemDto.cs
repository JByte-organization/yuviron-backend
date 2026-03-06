using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Artists.Queries.DTOs;

public record ArtistListItemDto(
    Guid Id,
    string Name,
    string? AvatarUrl,
    bool IsVerified,
    VerificationStatus VerificationStatus,
    bool IsDeleted,
    DateTime CreatedAt
);