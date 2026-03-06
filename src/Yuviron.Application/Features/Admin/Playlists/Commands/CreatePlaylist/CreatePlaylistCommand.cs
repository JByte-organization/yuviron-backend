using System;
using System.Collections.Generic;
using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.CreatePlaylist;

public sealed record TrackOrderInput(Guid TrackId, int Position);

public sealed record CreatePlaylistCommand(
    string Title,
    string? Description,
    string? CoverUrl,
    bool IsPublic,
    bool IsEditorial,
    List<TrackOrderInput> Tracks
) : IRequest<Guid>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}