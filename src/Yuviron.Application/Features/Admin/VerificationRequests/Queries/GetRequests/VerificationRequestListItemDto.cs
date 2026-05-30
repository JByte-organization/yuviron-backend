using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.VerificationRequests.Queries.GetRequests;

public record VerificationRequestListItemDto(
    Guid Id,
    Guid ArtistId,
    string ArtistName,
    Guid SubmittedByUserId,
    string SubmittedByUserEmail,
    ClaimRole ClaimedRole,
    VerificationRequestStatus Status,
    DateTime CreatedAt
);