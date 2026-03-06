using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Tracks.Commands.CreateTrack;

public sealed record CreateTrackCommand(
    Guid? AlbumId,
    string Title,
    int DurationMs,
    bool Explicit,
    string AudioStorageKey,
    string? PreviewStorageKey,
    string? CoverUrl,
    VisibilityStatus VisibilityStatus,
    List<Guid> ArtistIds,
    List<Guid> GenreIds,
    List<Guid> MoodIds
) : IRequest<Guid>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}




