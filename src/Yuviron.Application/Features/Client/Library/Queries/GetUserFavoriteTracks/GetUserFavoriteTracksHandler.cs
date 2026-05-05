using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common;
using Yuviron.Application.Extensions;

namespace Yuviron.Application.Features.Client.Library.Queries.GetUserFavoriteTracks;

public sealed class GetUserFavoriteTracksHandler : IRequestHandler<GetUserFavoriteTracksQuery, PaginatedList<UserFavoriteTrackDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetUserFavoriteTracksHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedList<UserFavoriteTrackDto>> Handle(GetUserFavoriteTracksQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var query = _context.UserSavedTracks
            .AsNoTracking()
            .Where(ust => ust.UserId == userId && !ust.Track.IsDeleted);

        var sortBy = request.SortBy?.ToLowerInvariant() ?? "savedat";
        var sortOrder = request.SortOrder?.ToLowerInvariant() ?? "desc";

        query = sortBy switch
        {
            "title" when sortOrder == "asc" => query.OrderBy(ust => ust.Track.Title),
            "title" => query.OrderByDescending(ust => ust.Track.Title),
            _ when sortOrder == "asc" => query.OrderBy(ust => ust.SavedAt),
            _ => query.OrderByDescending(ust => ust.SavedAt)
        };

        var projectedQuery = query.Select(ust => new UserFavoriteTrackDto(
            ust.TrackId,
            ust.Track.Title,
            ust.Track.TrackArtists.Select(ta => ta.Artist.Name).ToList(),
            ust.Track.AlbumId,
            ust.Track.Album != null ? ust.Track.Album.Title : "Unknown",
            ust.Track.CoverUrl ?? (ust.Track.Album != null ? ust.Track.Album.CoverUrl : null),
            ust.Track.DurationMs,
            ust.SavedAt
        ));

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
