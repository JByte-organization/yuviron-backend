using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.CreatePlaylist;

public sealed record CreatePlaylistCommand(
    Guid? OwnerUserId,
    string Title,
    string? Description,
    string? CoverUrl,
    PlaylistVisibility Visibility,
    bool IsEditorial
) : IRequest<Guid>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}