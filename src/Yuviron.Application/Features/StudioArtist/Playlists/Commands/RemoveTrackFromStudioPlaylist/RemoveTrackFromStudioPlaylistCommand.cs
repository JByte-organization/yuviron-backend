using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Playlists.Commands.RemoveTrackFromStudioPlaylist;

public sealed record RemoveTrackFromStudioPlaylistCommand(
    Guid PlaylistId, 
    Guid TrackId
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistManage;
}