using Yuviron.Application.Common.Models; 

namespace Yuviron.Application.Features.Admin.Playlists.Queries.GetPlaylistTracks;

public record PlaylistTrackItemDto(
    Guid TrackId,
    string Title,
    List<SimpleArtistDto> Artists, 
    Guid AlbumId,      
    string AlbumTitle, 
    string? CoverUrl,        
    int DurationMs,           
    double Position,
    DateTime AddedAt
);