using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;
using Yuviron.Application.Features.StudioArtist.Tracks.Queries.DTOs;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Queries.GetStudioTracks;

public sealed class GetStudioTracksHandler : IRequestHandler<GetStudioTracksQuery, PaginatedList<StudioTrackListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetStudioTracksHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<StudioTrackListItemDto>> Handle(GetStudioTracksQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var hasPermission = await _context.ArtistTeamMembers
            .HasManagementAccess(request.ArtistId, userId)
            .AnyAsync(cancellationToken);

        if (!hasPermission) throw new ForbiddenException("No access to this artist's tracks.");

        var query = _context.Tracks.AsNoTracking()
            .Where(t => t.TrackArtists.Any(ta => ta.ArtistId == request.ArtistId));

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(t => t.Title.Contains(request.SearchTerm));

        var sortedQuery = query.ApplySorting(
            request.SortBy,
            request.SortOrder,
            defaultSortBy: nameof(Track.CreatedAt),
            defaultDesc: true,
            mapping: new Dictionary<string, Expression<Func<Track, object>>>
            {
                ["AlbumTitle"] = t => t.Album != null ? t.Album.Title : "Unknown Album"
            });

        var projectedQuery = sortedQuery.Select(t => new StudioTrackListItemDto(
            t.Id,
            t.AlbumId,
            t.Album != null ? t.Album.Title : "Unknown Album",
            t.AlbumPosition,
            t.Title,
            t.TrackArtists
                .OrderBy(ta => ta.Role == ArtistRole.Main ? 0 : 1)
                .Select(ta => ta.Artist.Name)
                .ToList(),
            t.DurationMs,
            t.Explicit,
            t.CoverUrl ?? (t.Album != null ? t.Album.CoverUrl : null),
            t.ProcessingStatus,
            t.VisibilityStatus,
            t.PlayCount,
            t.CreatedAt,
            t.UpdatedAt
        ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}