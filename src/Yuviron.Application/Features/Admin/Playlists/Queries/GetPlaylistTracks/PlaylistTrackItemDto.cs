namespace Yuviron.Application.Features.Admin.Playlists.Queries.GetPlaylistTracks;

public record PlaylistTrackItemDto(
    Guid TrackId,
    string Title,
    string? ArtistName,
    int Position,
    DateTime AddedAt
);