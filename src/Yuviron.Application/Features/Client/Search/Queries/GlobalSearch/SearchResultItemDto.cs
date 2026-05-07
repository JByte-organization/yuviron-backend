using Yuviron.Application.Common.Models;

namespace Yuviron.Application.Features.Client.Search.Queries.GlobalSearch;

public record SearchTrackDto(
    Guid Id, 
    string Title, 
    IEnumerable<TrackArtistDto> Artists,
    string? CoverUrl
);

public record SearchArtistDto(
    Guid Id, 
    string Name, 
    string? AvatarUrl
);

public record SearchPlaylistDto(
    Guid Id, 
    string Title, 
    string CreatorName,
    string? CoverUrl
);