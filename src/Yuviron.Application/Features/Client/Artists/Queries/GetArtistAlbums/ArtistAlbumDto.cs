
namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistAlbums;

public record ArtistAlbumDto(
    Guid Id,
    string Title,
    string? CoverUrl,
    int ReleaseYear
);