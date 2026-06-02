using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Albums.Commands.DeleteAlbum;

public sealed record DeleteAlbumCommand(Guid AlbumId) : IRequest, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistManage;
}