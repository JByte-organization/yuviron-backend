using System;
using MediatR;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Common;
using Yuviron.Application.Features.Admin.Tracks.Queries.DTOs;
using Yuviron.Application.Features.Admin.Albums.Queries.DTOs; // Если PaginatedList лежит там
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Admin.Tracks.Queries.GetTracks;

public sealed record GetTracksQuery(
    string? SearchTerm,
    Guid? AlbumId, // <-- Фильтр по конкретному альбому!
    VisibilityStatus? Status,
    bool IncludeDeleted,
    int Page = 1,
    int PageSize = 20
) : IRequest<PaginatedList<TrackListItemDto>>, ISecuredRequest
{
    public AppPermission RequiredPermission => AppPermission.ManageCatalog;
}