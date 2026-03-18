using System;
using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Tracks.Queries.DTOs;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Tracks.Queries.GetTracks;

public sealed record GetTracksQuery(
    string? SearchTerm,
    Guid? AlbumId,
    VisibilityStatus? Status,
    int Page = 1,
    int PageSize = 20
) : PaginatedQuery(SearchTerm, Page, PageSize), 
    IRequest<PaginatedList<TrackListItemDto>>, 
    ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.AccessAdminPanel;
}