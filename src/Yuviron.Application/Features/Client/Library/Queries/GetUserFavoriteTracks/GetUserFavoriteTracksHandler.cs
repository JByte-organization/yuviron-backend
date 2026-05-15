using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common;
using Yuviron.Application.Common.Models;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;

namespace Yuviron.Application.Features.Client.Library.Queries.GetUserFavoriteTracks;

public sealed class GetUserFavoriteTracksHandler : IRequestHandler<GetUserFavoriteTracksQuery, PaginatedList<UserFavoriteTrackDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly TimeProvider _timeProvider;

    public GetUserFavoriteTracksHandler(
        IApplicationDbContext context, 
        ICurrentUserService currentUserService,
        TimeProvider timeProvider) 
    {
        _context = context;
        _currentUserService = currentUserService;
        _timeProvider = timeProvider;
    }

    public async Task<PaginatedList<UserFavoriteTrackDto>> Handle(GetUserFavoriteTracksQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
                     ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var query = _context.UserSavedTracks
            .AsNoTracking()
            .Where(ust => ust.UserId == userId && 
                          _context.Tracks.AvailableForPublic(utcNow).Any(t => t.Id == ust.TrackId));

        var sortedQuery = query.ApplySorting(
            request.SortBy,
            request.SortOrder,
            defaultSortBy: nameof(UserSavedTrack.SavedAt),
            defaultDesc: true,
            mapping: new Dictionary<string, Expression<Func<UserSavedTrack, object>>>
            {
                ["Title"] = ust => ust.Track.Title,
                ["AlbumTitle"] = ust => ust.Track.Album != null ? ust.Track.Album.Title : "Unknown",
                ["DurationMs"] = ust => ust.Track.DurationMs,
                ["SavedAt"] = ust => ust.SavedAt
            });

        var projectedQuery = sortedQuery.Select(ust => new UserFavoriteTrackDto(
            ust.TrackId,
            ust.Track.Title,
            ust.Track.TrackArtists
                .Where(ta => !ta.Artist.IsDeleted)
                .Select(ta => new TrackArtistDto(ta.Artist.Id, ta.Artist.Name, ta.Role)),
            ust.Track.AlbumId,
            ust.Track.Album != null ? ust.Track.Album.Title : "Unknown",
            ust.Track.CoverUrl ?? (ust.Track.Album != null ? ust.Track.Album.CoverUrl : null),
            ust.Track.DurationMs,
            ust.SavedAt
        ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
