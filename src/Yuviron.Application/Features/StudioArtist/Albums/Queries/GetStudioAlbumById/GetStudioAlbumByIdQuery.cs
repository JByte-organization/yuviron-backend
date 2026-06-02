using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Features.StudioArtist.Albums.Queries.DTOs;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Albums.Queries.GetStudioAlbumById;

public sealed record GetStudioAlbumByIdQuery(Guid AlbumId) : IRequest<StudioAlbumDetailsDto>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessArtistPanel;
}