using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.RemoveTrackFromPlaylist;

public sealed record RemoveTrackFromPlaylistCommand(
    Guid PlaylistId, 
    Guid TrackId
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}