using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Tracks.Commands.CreateTrack;

public sealed record CreateTrackCommand(
    Guid AlbumId,//avatar, name, ownerartist
    int AlbumPosition,
    string Title,
    bool Explicit,
    string AudioStorageKey,
    string? CoverUrl,
    VisibilityStatus VisibilityStatus,
    List<Guid> ArtistIds,//avatar, name, email 
    List<Guid> GenreIds,//avatar and name
    List<Guid> MoodIds//avatar and name
) : IRequest<Guid>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}




