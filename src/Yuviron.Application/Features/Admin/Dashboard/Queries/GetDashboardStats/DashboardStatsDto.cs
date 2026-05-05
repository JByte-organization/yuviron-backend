
namespace Yuviron.Application.Features.Admin.Dashboard.Queries.GetDashboardStats;

public record AdminDashboardDto(
    DashboardSummaryDto Summary,
    List<RecentUserDto> RecentUsers,
    List<TopEntityDto> TopGenres,
    List<TopEntityDto> TopMoods,
    List<PopularAlbumDto> PopularAlbums,
    List<TopEntityDto> TopArtists
);

public record DashboardSummaryDto(
    int TotalTracks,
    int TotalArtists,
    int TotalAlbums,
    int TotalUsers,
    int NewUsersLast24h,
    int TotalPremiumUsers,
    long TotalPlays    
);

public record RecentUserDto(
    Guid Id,
    string Email,
    string FirstName, 
    string? AvatarUrl,  
    DateTime CreatedAt
);

public record TopEntityDto(
    Guid Id, 
    string Name, 
    string? CoverUrl,   
    long TotalPlays
);

public record PopularAlbumDto(
    Guid Id, 
    string Title, 
    string? CoverUrl, 
    long TotalPlays
);

//top entity artist+