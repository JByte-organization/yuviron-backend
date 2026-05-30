using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.VerificationRequests.Queries.GetRequestById;

public record VerificationRequestDetailDto(
    Guid Id,
    Guid ArtistId,
    string ArtistName,
    string? ArtistAvatarUrl,
    Guid SubmittedByUserId,
    string SubmittedByUserEmail,
    ClaimRole ClaimedRole,
    string OfficialEmail,
    string Links,
    string? ProofFileUrl,
    string? Message,
    VerificationRequestStatus Status,
    string? AdminNote,
    DateTime CreatedAt,
    DateTime UpdatedAt
);