using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common;
using Yuviron.Application.Common.Models;
using Yuviron.Application.Extensions;

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

        var projectedQuery = query.Select(ust => new UserFavoriteTrackDto(
            ust.TrackId,
            ust.Track.Title,
            ust.Track.TrackArtists.Select(ta => new TrackArtistDto(ta.Artist.Id, ta.Artist.Name, ta.Role)),
            ust.Track.AlbumId,
            ust.Track.Album != null ? ust.Track.Album.Title : "Unknown",
            ust.Track.CoverUrl ?? (ust.Track.Album != null ? ust.Track.Album.CoverUrl : null),
            ust.Track.DurationMs,
            ust.SavedAt
        ));

        var sortedQuery = projectedQuery.ApplySorting(
            request.SortBy,
            request.SortOrder,
            defaultSortBy: nameof(UserFavoriteTrackDto.SavedAt),
            defaultDesc: true);

        return await sortedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}