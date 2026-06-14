using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Playlists.Queries.GetStudioPlaylistTracks;

public sealed class GetStudioPlaylistTracksHandler : IRequestHandler<GetStudioPlaylistTracksQuery, PaginatedList<StudioPlaylistTrackItemDto>>
{
    private readonly ICatalogContext _catalogContext;
    private readonly ILibraryContext _libraryContext;
    private readonly ICurrentUserService _currentUser;

    public GetStudioPlaylistTracksHandler(ICatalogContext catalogContext, ILibraryContext libraryContext, ICurrentUserService currentUser)
    {
        _catalogContext = catalogContext;
        _libraryContext = libraryContext;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<StudioPlaylistTrackItemDto>> Handle(GetStudioPlaylistTracksQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var playlistInfo = await _libraryContext.Playlists
            .AsNoTracking()
            .Where(p => p.Id == request.PlaylistId && !p.IsDeleted)
            .Select(p => new { p.ArtistId })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Playlist), request.PlaylistId);

        if (playlistInfo.ArtistId == null) throw new ForbiddenException("Not an artist playlist.");

        var hasPermission = await _catalogContext.ArtistTeamMembers
            .HasViewerAccess(playlistInfo.ArtistId.Value, userId)
            .AnyAsync(cancellationToken);

        if (!hasPermission) throw new ForbiddenException("No access to view this playlist's tracks.");

        var query = _libraryContext.PlaylistTracks
            .AsNoTracking()
            .Where(pt => pt.PlaylistId == request.PlaylistId)
            .OrderBy(pt => pt.Position);

        var projectedQuery = query.Select(pt => new StudioPlaylistTrackItemDto(
            pt.TrackId,
            pt.Track.Title,
            pt.Track.TrackArtists
                .OrderBy(ta => ta.Role == ArtistRole.Main ? 0 : 1)
                .Select(ta => ta.Artist.Name),
            pt.Track.Album != null ? pt.Track.Album.Title : "Unknown",
            pt.Track.CoverUrl ?? (pt.Track.Album != null ? pt.Track.Album.CoverUrl : null),
            pt.Track.DurationMs,
            pt.Position,
            pt.Track.ProcessingStatus,
            pt.Track.VisibilityStatus,
            pt.AddedAt
        ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
