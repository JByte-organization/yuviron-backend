using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Playlists.Commands.ChangeTrackPositionInStudioPlaylist;

public sealed record ChangeTrackPositionInStudioPlaylistCommand(
    Guid PlaylistId, 
    Guid TrackId, 
    double NewPosition
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistManage;
}