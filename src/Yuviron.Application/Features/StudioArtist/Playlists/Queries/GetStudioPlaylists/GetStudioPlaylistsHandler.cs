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
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;
using System;

namespace Yuviron.Application.Features.StudioArtist.Playlists.Queries.GetStudioPlaylists;

public sealed class GetStudioPlaylistsHandler : IRequestHandler<GetStudioPlaylistsQuery, PaginatedList<StudioPlaylistListItemDto>>
{
    private readonly ICatalogContext _catalogContext;
    private readonly ILibraryContext _libraryContext;
    private readonly ICurrentUserService _currentUser;

    public GetStudioPlaylistsHandler(ICatalogContext catalogContext, ILibraryContext libraryContext, ICurrentUserService currentUser)
    {
        _catalogContext = catalogContext;
        _libraryContext = libraryContext;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<StudioPlaylistListItemDto>> Handle(GetStudioPlaylistsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var trackId = request.TrackId;

        var hasPermission = await _catalogContext.ArtistTeamMembers
            .HasViewerAccess(request.ArtistId, userId)
            .AnyAsync(cancellationToken);

        if (!hasPermission) throw new ForbiddenException("No access to view this artist's playlists.");

        var query = _libraryContext.Playlists
            .AsNoTracking()
            .Where(p => p.ArtistId == request.ArtistId && !p.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(p => p.Title.Contains(request.SearchTerm));

        var sortedQuery = query.ApplySorting(
            request.SortBy,
            request.SortOrder,
            defaultSortBy: nameof(Playlist.CreatedAt),
            defaultDesc: true);

        var projectedQuery = sortedQuery.Select(p => new StudioPlaylistListItemDto(
            p.Id,
            p.Title,
            p.CoverUrl,
            p.Visibility,
            p.CreatedAt,
            trackId.HasValue && p.PlaylistTracks.Any(pt => pt.TrackId == trackId.Value)
        ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
