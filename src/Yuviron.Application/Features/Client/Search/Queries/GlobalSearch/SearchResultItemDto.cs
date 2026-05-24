using Yuviron.Application.Common.Models;

namespace Yuviron.Application.Features.Client.Search.Queries.GlobalSearch;

public sealed record SearchTrackDto(
    Guid Id, 
    string Title, 
    IEnumerable<TrackArtistDto> Artists,
    string? CoverUrl
);

public sealed record SearchArtistDto(
    Guid Id, 
    string Name, 
    string? AvatarUrl
);

public sealed record SearchAlbumDto(
    Guid Id,
    string Title,
    IEnumerable<TrackArtistDto> Artists,
    string? CoverUrl,
    int ReleaseYear
);

public sealed record SearchPlaylistDto(
    Guid Id,
    string Title,
    string CreatorName,
    string? CoverUrl
);
