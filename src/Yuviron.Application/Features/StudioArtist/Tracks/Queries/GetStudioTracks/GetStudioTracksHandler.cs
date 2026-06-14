using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.StudioArtist.Tracks.Queries.DTOs;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;
using System;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Queries.GetStudioTracks;

public sealed class GetStudioTracksHandler : IRequestHandler<GetStudioTracksQuery, PaginatedList<StudioTrackListItemDto>>
{
    private readonly ICatalogContext _catalogContext;
    private readonly ICurrentUserService _currentUser;

    public GetStudioTracksHandler(ICatalogContext catalogContext, ICurrentUserService currentUser)
    {
        _catalogContext = catalogContext;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<StudioTrackListItemDto>> Handle(GetStudioTracksQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var hasPermission = await _catalogContext.ArtistTeamMembers
            .HasViewerAccess(request.ArtistId, userId)
            .AnyAsync(cancellationToken);

        if (!hasPermission) throw new ForbiddenException("No access to this artist's tracks.");

        var query = _catalogContext.Tracks.AsNoTracking()
            .Where(t => t.TrackArtists.Any(ta => ta.ArtistId == request.ArtistId) && !t.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(t => t.Title.Contains(request.SearchTerm));

        var sortedQuery = query.ApplySorting(
            request.SortBy,
            request.SortOrder,
            defaultSortBy: nameof(Track.CreatedAt),
            defaultDesc: true);

        var projectedQuery = sortedQuery.Select(t => new StudioTrackListItemDto(
            t.Id,
            t.Title,
            t.CoverUrl ?? (t.Album != null ? t.Album.CoverUrl : null),
            t.DurationMs,
            t.ProcessingStatus,
            t.VisibilityStatus,
            t.PlayCount, 
            t.CreatedAt
        ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
