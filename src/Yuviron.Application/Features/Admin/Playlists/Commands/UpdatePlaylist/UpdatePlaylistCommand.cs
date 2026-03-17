using System;
using System.Collections.Generic;
using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.UpdatePlaylist;

public sealed record UpdatePlaylistCommand(
    Guid Id,
    string Title,
    string? Description,
    string? CoverUrl,
    PlaylistVisibility Visibility
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}