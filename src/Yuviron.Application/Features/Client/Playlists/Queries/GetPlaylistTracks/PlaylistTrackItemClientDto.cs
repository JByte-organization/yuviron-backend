using Yuviron.Application.Common.Models;

namespace Yuviron.Application.Features.Client.Playlists.Queries.GetPlaylistTracks;

public record PlaylistTrackItemClientDto(
    Guid TrackId,
    string Title,
    List<SimpleArtistDto> Artists, 
    Guid AlbumId,      
    string? CoverUrl,        
    int DurationMs,           
    double Position,
    DateTime AddedAt, 
    bool IsSaved = false
);