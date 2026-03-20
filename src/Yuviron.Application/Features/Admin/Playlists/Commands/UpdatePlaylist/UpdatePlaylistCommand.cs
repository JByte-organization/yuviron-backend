using System;
using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.UpdatePlaylist;

public sealed record UpdatePlaylistCommand(
    Guid Id,
    Guid? OwnerUserId,
    string Title,
    string? Description,
    string? CoverUrl,
    PlaylistVisibility Visibility,
    bool IsEditorial  
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}