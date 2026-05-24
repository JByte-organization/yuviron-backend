using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Albums.Commands.UpdateAlbum;

public sealed record UpdateAlbumCommand(
    Guid AlbumId,
    string Title,
    string? Description,
    Guid? CoverFileId,
    DateTime ReleaseDate,
    ReleaseType ReleaseType,
    VisibilityStatus VisibilityStatus,
    DateTime? ScheduledPublishAt,
    List<Guid> ArtistIds 
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}
