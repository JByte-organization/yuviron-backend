using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Playlists.Commands.UpdatePlaylist;

public sealed record UpdateStudioPlaylistCommand(
    Guid PlaylistId,
    string? Title,
    string? Description,
    Guid? CoverFileId,
    PlaylistVisibility? Visibility
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistManage;
}