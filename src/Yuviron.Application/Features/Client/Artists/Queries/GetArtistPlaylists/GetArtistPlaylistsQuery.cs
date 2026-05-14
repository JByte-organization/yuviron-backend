using MediatR;
using System;
using Yuviron.Application.Common;

namespace Yuviron.Application.Features.Client.Artists.Queries.GetArtistPlaylists;

public sealed record GetArtistPlaylistsQuery(
    Guid ArtistId,
    string? SortBy = null,     
    string? SortOrder = null,  
    int Page = 1,
    int PageSize = 10
) : PaginatedQuery(null, SortBy, SortOrder, Page, PageSize), 
    IRequest<PaginatedList<ArtistPlaylistDto>>;