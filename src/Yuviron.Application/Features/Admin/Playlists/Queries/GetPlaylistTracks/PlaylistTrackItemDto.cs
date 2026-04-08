namespace Yuviron.Application.Features.Admin.Playlists.Queries.GetPlaylistTracks;

public record PlaylistTrackItemDto(
    Guid TrackId,//avatar,name, ownerArtist
    string Title,
    List<string> ArtistNames,
    Guid AlbumId,      
    string AlbumTitle, 
    string? CoverUrl,        
    int DurationMs,           
    int Position,
    DateTime AddedAt
);