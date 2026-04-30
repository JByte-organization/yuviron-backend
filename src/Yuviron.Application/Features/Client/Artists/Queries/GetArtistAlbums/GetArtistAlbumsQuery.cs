using MediatR;
using System;
using Yuviron.Application.Common;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistAlbums;

public sealed record GetArtistAlbumsQuery(
    Guid ArtistId, 
    string? SearchTerm = null,
    int Page = 1, 
    int PageSize = 10
) : PaginatedQuery(SearchTerm, Page, PageSize), 
    IRequest<PaginatedList<ArtistAlbumDto>>;