using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Playlists.Commands.CreatePlaylist;

public sealed record CreatePlaylistCommand(
    Guid? OwnerUserId,
    Guid? ArtistId,
    string Title,
    string? Description,
    Guid? CoverFileId,
    PlaylistVisibility Visibility,
    bool IsEditorial
) : IRequest<Guid>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}