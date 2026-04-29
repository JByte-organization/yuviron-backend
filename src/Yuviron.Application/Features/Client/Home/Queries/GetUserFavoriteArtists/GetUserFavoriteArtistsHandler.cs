using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;

namespace Yuviron.Application.Features.Client.Home.Queries.GetUserFavoriteArtists;

public sealed class GetUserFavoriteArtistsHandler : IRequestHandler<GetUserFavoriteArtistsQuery, List<FavoriteArtistDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly TimeProvider _timeProvider;

    public GetUserFavoriteArtistsHandler(
        IApplicationDbContext context, 
        ICurrentUserService currentUserService,
        TimeProvider timeProvider)
    {
        _context = context;
        _currentUserService = currentUserService;
        _timeProvider = timeProvider;
    }

    public async Task<List<FavoriteArtistDto>> Handle(GetUserFavoriteArtistsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId 
                     ?? throw new UnauthorizedAccessException("User is not authenticated.");

        var minDate = _timeProvider.GetUtcNow().UtcDateTime.AddDays(-30);

        var topArtistIds = await _context.ListeningEvents
            .Where(le => le.UserId == userId && 
                         le.PlayedAt >= minDate && 
                         le.MsPlayed >= 30000) 
            .Join(_context.TrackArtists,
                le => le.TrackId,
                ta => ta.TrackId,
                (le, ta) => ta.ArtistId)
            .GroupBy(artistId => artistId)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        if (!topArtistIds.Any()) return new List<FavoriteArtistDto>();

        var dbArtists = await _context.Artists
            .AsNoTracking()
            .Where(a => topArtistIds.Contains(a.Id) && !a.IsDeleted)
            .Select(a => new FavoriteArtistDto(
                a.Id,
                a.Name,
                a.AvatarUrl,
                _context.UserFollowArtists
                    .Count(ufa => ufa.ArtistId == a.Id) 
            ))
            .ToListAsync(cancellationToken);

        return topArtistIds
            .Select(id => dbArtists.FirstOrDefault(a => a.Id == id))
            .Where(a => a != null)
            .ToList()!;
    }
}