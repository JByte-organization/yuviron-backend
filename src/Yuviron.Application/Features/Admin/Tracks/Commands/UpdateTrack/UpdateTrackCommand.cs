using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Tracks.Commands.UpdateTrack;

public sealed record UpdateTrackCommand(
    Guid TrackId,
    Guid AlbumId,
    int AlbumPosition,
    string Title,
    bool Explicit,
    string AudioStorageKey,
    string? CoverUrl,
    VisibilityStatus VisibilityStatus,
    List<Guid> ArtistIds,
    List<Guid> GenreIds,
    List<Guid> MoodIds,
    string? Isrc = null
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}




