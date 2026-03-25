using System;
using System.Collections.Generic;

namespace Yuviron.Application.Features.Admin.Dashboard.Queries.GetDashboardStats;

public record AdminDashboardDto(
    DashboardSummaryDto Summary,
    List<RecentUserDto> RecentUsers,
    List<TopEntityDto> TopGenres,
    List<TopEntityDto> TopMoods,
    List<PopularAlbumDto> PopularAlbums
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
    string DisplayName, //update 23:15
    string? AvatarUrl,  
    DateTime CreatedAt
);

public record TopEntityDto(
    Guid Id, 
    string Name, 
    string? CoverUrl,   
    int TotalPlays 
);

public record PopularAlbumDto(
    Guid Id, 
    string Title, 
    string? CoverUrl, 
    int TotalPlays 
);

//top entity artist+