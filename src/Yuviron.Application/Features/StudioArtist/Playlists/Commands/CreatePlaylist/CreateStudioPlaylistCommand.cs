using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Playlists.Commands.CreatePlaylist;

public sealed record CreateStudioPlaylistCommand(
    Guid ArtistId,
    string Title,
    string? Description,
    Guid? CoverFileId,
    PlaylistVisibility Visibility
) : IRequest<Guid>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistManage;
}