namespace Yuviron.Application.Features.Admin.Dashboard.Queries.GetDashboardStats;

public record AdminDashboardDto(
    DashboardSummaryDto Summary,
    List<RecentActivityDto> RecentTracks,
    List<PendingVerificationDto> PendingArtists
);

public record DashboardSummaryDto(
    int TotalTracks,
    int TotalArtists,
    int TotalAlbums,
    int TotalUsers,
    int NewUsersLast24h,
    int PendingVerificationCount
);

public record RecentActivityDto(
    Guid Id,
    string Title,
    string? CoverUrl,
    DateTime CreatedAt
);

public record PendingVerificationDto(
    Guid ArtistId,
    string Name,
    string? AvatarUrl,
    DateTime RequestedAt
);