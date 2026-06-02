using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Albums.Queries.GetStudioAlbums;

public sealed record GetStudioAlbumsQuery(
    Guid ArtistId,
    string? SearchTerm = null,
    string? SortBy = null,    
    string? SortOrder = null,
    int Page = 1,
    int PageSize = 20
) : PaginatedQuery(SearchTerm, SortBy, SortOrder, Page, PageSize),
    IRequest<PaginatedList<StudioAlbumListItemDto>>, 
    ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessArtistPanel;
}