using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Albums.Commands.CreateAlbum;

public sealed record CreateAlbumCommand(
    string Title,
    string? Description,
    Guid? CoverFileId,
    DateTime ReleaseDate,
    ReleaseType ReleaseType,
    VisibilityStatus VisibilityStatus,
    DateTime? ScheduledPublishAt,
    List<Guid> ArtistIds 
) : IRequest<Guid>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}