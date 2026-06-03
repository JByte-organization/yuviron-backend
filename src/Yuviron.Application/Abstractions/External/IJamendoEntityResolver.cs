using Yuviron.Domain.Entities;

namespace Yuviron.Application.Abstractions.Services.Jamendo;

public interface IJamendoEntityResolver
{
    Task<Artist> ResolveArtistAsync(string name, string jamendoId, CancellationToken cancellationToken);
    Task<Album> ResolveAlbumAsync(string title, string jamendoId, Guid artistId, Guid? coverFileId, Guid adminId, CancellationToken cancellationToken);
    Task<int> GetNextAlbumPositionAsync(Guid albumId, CancellationToken cancellationToken);
}