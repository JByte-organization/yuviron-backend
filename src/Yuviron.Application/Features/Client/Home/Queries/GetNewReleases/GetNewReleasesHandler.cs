using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Common.Models;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Client.Home.Queries.GetNewReleases;

public sealed class GetNewReleasesHandler : IRequestHandler<GetNewReleasesQuery, List<NewReleaseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICacheService _cache;
    private readonly ICurrentUserService _currentUser;

    public GetNewReleasesHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        ICacheService cache,
        ICurrentUserService currentUser)
    {
        _context = context;
        _timeProvider = timeProvider;
        _cache = cache;
        _currentUser = currentUser;
    }

    public async Task<List<NewReleaseDto>> Handle(GetNewReleasesQuery request, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var releases = await _context.Albums
            .AsNoTracking()
            .AvailableForPublic(utcNow)
            .Where(a => a.Tracks.Any(t => !t.IsDeleted 
                                          && t.VisibilityStatus == VisibilityStatus.Published 
                                          && t.ProcessingStatus == TrackProcessingStatus.Ready))
            .OrderByDescending(a => a.ReleaseDate) 
            .ThenByDescending(a => a.CreatedAt)
            .Take(request.Limit)
            .Select(a => new NewReleaseDto(
                a.Id,
                a.Title,
                a.AlbumArtists.Select(aa => new TrackArtistDto(aa.Artist.Id, aa.Artist.Name, aa.Role)),
                a.CoverUrl,
                a.Tracks.Count(t => !t.IsDeleted 
                                    && t.VisibilityStatus == VisibilityStatus.Published
                                    && t.ProcessingStatus == TrackProcessingStatus.Ready),
                false 
            ))
            .ToListAsync(cancellationToken);

        return await releases.EnrichWithCacheAsync(
            _cache, 
            _currentUser.UserId, 
            "saved_albums", 
            x => x.Id, 
            (x, saved) => x with { IsSaved = saved }, 
            cancellationToken);
    }
}