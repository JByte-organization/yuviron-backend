using System.Linq.Expressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common;
using Yuviron.Application.Common.Models;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Tracks.Queries.GetTracks;

public sealed class GetStudioArtistTracksHandler
    : IRequestHandler<GetStudioArtistTracksQuery, PaginatedList<StudioArtistTrackListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetStudioArtistTracksHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PaginatedList<StudioArtistTrackListItemDto>> Handle(
        GetStudioArtistTracksQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var currentArtistId = await _context.ArtistTeamMembers
            .AsNoTracking()
            .Where(tm => tm.UserId == userId)
            .OrderBy(tm => tm.Role == ArtistTeamRole.Owner ? 0 :
                tm.Role == ArtistTeamRole.Manager ? 1 :
                tm.Role == ArtistTeamRole.Editor ? 2 :
                tm.Role == ArtistTeamRole.Viewer ? 3 : 4)
            .ThenByDescending(tm => tm.CreatedAt)
            .Select(tm => (Guid?)tm.ArtistId)
            .FirstOrDefaultAsync(cancellationToken);

        if (!currentArtistId.HasValue)
        {
            throw new NotFoundException(nameof(Artist), $"for user {userId}");
        }

        var query = _context.Tracks
            .AsNoTracking()
            .ForArtist(currentArtistId.Value);

        var sortedQuery = query.ApplySorting(
            request.SortBy,
            request.SortOrder,
            defaultSortBy: nameof(Track.CreatedAt),
            defaultDesc: true,
            mapping: new Dictionary<string, Expression<Func<Track, object>>>
            {
                ["AlbumTitle"] = t => t.Album != null ? t.Album.Title : "Unknown Album",
                ["ReleaseDate"] = t => t.Album != null ? t.Album.ReleaseDate : DateTime.MinValue
            });

        var projectedQuery = sortedQuery.Select(t => new StudioArtistTrackListItemDto(
            t.Id,
            t.AlbumId,
            t.Album != null ? t.Album.Title : "Unknown Album",
            t.Album != null ? t.Album.ReleaseDate : DateTime.MinValue,
            t.AlbumPosition,
            t.Title,
            t.TrackArtists
                .OrderBy(ta => ta.Role == ArtistRole.Main ? 0 : 1)
                .Select(ta => new TrackArtistDto(ta.ArtistId, ta.Artist.Name, ta.Role))
                .ToList(),
            t.DurationMs,
            t.Explicit,
            t.CoverUrl ?? (t.Album != null ? t.Album.CoverUrl : null),
            t.PlayCount,
            t.VisibilityStatus,
            t.ProcessingStatus,
            t.CreatedAt,
            t.UpdatedAt
        ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
