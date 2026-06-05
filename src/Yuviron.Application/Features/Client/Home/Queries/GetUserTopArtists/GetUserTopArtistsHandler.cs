using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;

namespace Yuviron.Application.Features.Client.Home.Queries.GetUserTopArtists;

public sealed class GetUserTopArtistsHandler : IRequestHandler<GetUserTopArtistsQuery, List<TopArtistDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly TimeProvider _timeProvider;
    private readonly ICacheService _cache;

    public GetUserTopArtistsHandler(
        IApplicationDbContext context, 
        ICurrentUserService currentUserService,
        TimeProvider timeProvider,
        ICacheService cache)
    {
        _context = context;
        _currentUserService = currentUserService;
        _timeProvider = timeProvider;
        _cache = cache;
    }

    public async Task<List<TopArtistDto>> Handle(GetUserTopArtistsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User is not authenticated.");
        var minDate = _timeProvider.GetUtcNow().UtcDateTime.AddDays(-30);

        var topArtistIds = await _context.ListeningEvents
            .Where(le => le.UserId == userId && le.PlayedAt >= minDate && le.MsPlayed >= 30000) 
            .Join(_context.TrackArtists.Where(ta => !ta.Track.IsDeleted && !ta.Artist.IsDeleted),
                le => le.TrackId,
                ta => ta.TrackId,
                (le, ta) => ta.ArtistId)
            .GroupBy(artistId => artistId)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        if (!topArtistIds.Any()) return new List<TopArtistDto>();

        var dbArtists = await _context.Artists
            .AsNoTracking()
            .Where(a => topArtistIds.Contains(a.Id) )
            .Select(a => new TopArtistDto(
                a.Id,
                a.Name,
                a.AvatarUrl,
                _context.UserFollowArtists.Count(ufa => ufa.ArtistId == a.Id),
                false 
            ))
            .ToListAsync(cancellationToken);

        var result = topArtistIds
            .Select(id => dbArtists.FirstOrDefault(a => a.Id == id))
            .Where(a => a is null == false)
            .Select(a => a!)
            .ToList();

        return await result.EnrichWithCacheAsync(
            _cache, 
            userId, 
            "followed_artists", 
            x => x.Id, 
            (x, followed) => x with { IsFollowed = followed }, 
            cancellationToken);
    }
}