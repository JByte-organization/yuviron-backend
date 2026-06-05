using Yuviron.Application.Common.Models;

namespace Yuviron.Application.Features.Client.Search.Queries.GlobalSearch;

public sealed record SearchTrackDto(
    Guid Id, 
    string Title, 
    IEnumerable<TrackArtistDto> Artists,
    string? CoverUrl, 
    bool IsSaved = false
);

public sealed record SearchArtistDto(
    Guid Id, 
    string Name, 
    string? AvatarUrl, 
    bool IsFollowed = false
);

public sealed record SearchAlbumDto(
    Guid Id,
    string Title,
    IEnumerable<TrackArtistDto> Artists,
    string? CoverUrl,
    int ReleaseYear, 
    bool IsSaved = false
);

public sealed record SearchPlaylistDto(
    Guid Id,
    string Title,
    string CreatorName,
    string? CoverUrl, 
    bool IsSaved = false
);
