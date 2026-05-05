using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common.Models;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Tracks.Commands.CreateTrack;

public sealed record CreateTrackCommand(
    Guid AlbumId,
    int AlbumPosition,
    string Title,
    bool Explicit,
    string AudioStorageKey,
    string? CoverUrl,
    VisibilityStatus VisibilityStatus,
    List<TrackArtistDto> Artists,
    List<Guid> GenreIds,
    List<Guid> MoodIds,
    string? Isrc = null
) : IRequest<Guid>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}