using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Albums.Queries.DTOs;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Albums.Queries.GetAlbums;

public sealed record GetStudioArtistAlbumsQuery(
    int Page = 1,
    int PageSize = 20
) : PaginatedQuery(null, null, null, Page, PageSize),
    IRequest<PaginatedList<AlbumListItemDto>>,
    ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.StudioArtistManage;
}
