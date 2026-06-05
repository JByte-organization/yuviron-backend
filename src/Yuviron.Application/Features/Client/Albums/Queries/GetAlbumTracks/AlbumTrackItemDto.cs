namespace Yuviron.Application.Features.Client.Albums.Queries.GetAlbumTracks;

public record AlbumTrackItemDto(
    Guid Id,
    string Title,
    int AlbumPosition, 
    bool IsSaved = false
);
