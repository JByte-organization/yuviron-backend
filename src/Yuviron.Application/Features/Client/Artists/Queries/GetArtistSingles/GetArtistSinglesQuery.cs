using MediatR;
using System;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Client.Artists.Queries.GetArtistAlbums;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistSingles;

public sealed record GetArtistSinglesQuery(
    Guid ArtistId,
    string? SortBy = null,    
    string? SortOrder = null, 
    int Page = 1,
    int PageSize = 10
) : PaginatedQuery(null, SortBy, SortOrder, Page, PageSize), 
    IRequest<PaginatedList<ArtistAlbumDto>>;