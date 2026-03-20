using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

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
    List<Guid> MoodIds
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}




