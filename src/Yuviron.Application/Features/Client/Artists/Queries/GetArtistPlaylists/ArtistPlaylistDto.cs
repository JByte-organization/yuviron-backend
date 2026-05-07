namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistPlaylists;

public sealed record ArtistPlaylistDto(
    Guid Id,
    string Title,
    string CreatorName,
    string? CoverUrl,
    int TracksCount,
    bool IsEditorial
);
