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

        IQueryable<UserFavoriteTrackDto> projectedQuery = _context.UserSavedTracks
            .AsNoTracking()
            .Where(ust => ust.UserId == userId && !ust.Track.IsDeleted)
            .Select(ust => new UserFavoriteTrackDto(
                ust.TrackId,
                ust.Track.Title,
                ust.Track.TrackArtists.Select(ta => ta.Artist.Name).ToList(),
                ust.Track.AlbumId,
                ust.Track.Album != null ? ust.Track.Album.Title : "Unknown",
                ust.Track.CoverUrl ?? (ust.Track.Album != null ? ust.Track.Album.CoverUrl : null),
                ust.Track.DurationMs,
                ust.SavedAt
            ));

        var sortBy = request.SortBy?.ToLowerInvariant() ?? "savedat";
        var sortOrder = request.SortOrder?.ToLowerInvariant() ?? "desc";

        projectedQuery = sortBy switch
        {
            "title" when sortOrder == "asc" => projectedQuery.OrderBy(t => t.Title),
            "title" => projectedQuery.OrderByDescending(t => t.Title),
            _ when sortOrder == "asc" => projectedQuery.OrderBy(t => t.SavedAt),
            _ => projectedQuery.OrderByDescending(t => t.SavedAt)
        };

        return await projectedQuery.ToPaginatedListAsync(request.Page, request.PageSize, cancellationToken);
    }
}
