using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.UpdatePlaylist;

public sealed record UpdatePlaylistCommand(
    Guid Id,
    Guid? OwnerUserId,
    Guid? ArtistId, 
    string Title,
    string? Description,
    Guid? CoverFileId,
    PlaylistVisibility Visibility,
    bool IsEditorial
) : IRequest<Unit>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}