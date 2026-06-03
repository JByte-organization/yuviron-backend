using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Finance.Queries.GetArtistPayoutRequests;

public record ArtistPayoutRequestDto(
    Guid Id,
    decimal RequestedAmount,
    PayoutStatus Status,
    DateTime RequestedAt,
    string? DecisionNote
);