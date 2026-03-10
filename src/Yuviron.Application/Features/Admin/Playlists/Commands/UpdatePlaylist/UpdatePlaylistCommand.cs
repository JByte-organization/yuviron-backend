using System;
using System.Collections.Generic;
using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.UpdatePlaylist;

public sealed record UpdateTrackOrderInput(Guid TrackId, int Position);
public sealed record UpdatePlaylistCommand(
    Guid Id,
    string Title,
    string? Description,
    string? CoverUrl,
    bool IsPublic,
    List<UpdateTrackOrderInput> Tracks
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}