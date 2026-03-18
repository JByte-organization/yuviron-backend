using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Artists.Queries.DTOs;

public record ArtistListItemDto(
    Guid Id,
    string Name,
    string? AvatarUrl,     
    string? OwnerEmail,       
    VerificationStatus VerificationStatus,
    int TotalAlbums,       
    DateTime CreatedAt,
    DateTime UpdatedAt
);