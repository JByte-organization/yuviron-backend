using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Dashboard.Queries.GetDashboardStats;

public sealed record StudioArtistDashboardDto(
    StudioArtistInfoDto Artist,
    StudioArtistSummaryDto Summary,
    StudioArtistSubscriptionDto? ActiveSubscription
);

public sealed record StudioArtistInfoDto(
    Guid Id,
    string Name,
    string? AvatarUrl,
    string? BannerUrl,
    VerificationStatus VerificationStatus,
    ArtistTeamRole CurrentUserRole
);

public sealed record StudioArtistSummaryDto(
    int TotalAlbums,
    int TotalTracks,
    int TotalFollowers,
    int TeamMembersCount,
    long TotalPlays,
    int MonthlyListenersCount
);

public sealed record StudioArtistSubscriptionDto(
    Guid Id,
    Guid PlanId,
    string PlanName,
    DateTime StartAt,
    DateTime EndAt,
    bool IsAutoRenewing
);
