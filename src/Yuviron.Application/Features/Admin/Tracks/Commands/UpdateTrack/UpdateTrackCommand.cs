using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common.Models;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Tracks.Commands.UpdateTrack;

public sealed record UpdateTrackCommand(
    Guid TrackId,
    Guid AlbumId,
    int AlbumPosition,
    string Title,
    bool Explicit,
    Guid? AudioFileId,
    Guid? CoverFileId, 
    VisibilityStatus VisibilityStatus,
    List<TrackArtistDto> Artists,
    List<Guid> GenreIds,
    List<Guid> MoodIds,
    string? Isrc = null
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}