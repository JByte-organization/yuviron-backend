using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Artists.Queries.DTOs;

public record ArtistListItemDto(
    Guid Id,
    string Name,
    VerificationStatus VerificationStatus,
    DateTime CreatedAt
);