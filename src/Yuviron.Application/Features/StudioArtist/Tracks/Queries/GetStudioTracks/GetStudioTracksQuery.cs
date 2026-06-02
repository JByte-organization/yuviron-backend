using MediatR;
using System;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Features.StudioArtist.Tracks.Queries.DTOs;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Queries.GetStudioTracks;

public sealed record GetStudioTracksQuery(
    Guid ArtistId,
    string? SearchTerm = null,
    string? SortBy = null,    
    string? SortOrder = null,
    int Page = 1,
    int PageSize = 20
) : PaginatedQuery(SearchTerm, SortBy, SortOrder, Page, PageSize),
    IRequest<PaginatedList<StudioTrackListItemDto>>, 
    ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessArtistPanel;
}