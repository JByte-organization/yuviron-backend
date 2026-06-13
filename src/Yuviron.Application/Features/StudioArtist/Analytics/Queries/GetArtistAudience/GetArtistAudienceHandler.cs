using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Analytics;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetArtistAudience;

public sealed class GetArtistAudienceHandler : IRequestHandler<GetArtistAudienceQuery, ArtistAudienceDashboardDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cacheService;
    private readonly TimeProvider _timeProvider;
    private readonly IAnalyticsRepository _analyticsRepository;

    public GetArtistAudienceHandler(
        IApplicationDbContext context, 
        ICurrentUserService currentUser,
        ICacheService cacheService,
        TimeProvider timeProvider,
        IAnalyticsRepository analyticsRepository)
    {
        _context = context;
        _currentUser = currentUser;
        _cacheService = cacheService;
        _timeProvider = timeProvider;
        _analyticsRepository = analyticsRepository;
    }

    public async Task<ArtistAudienceDashboardDto> Handle(GetArtistAudienceQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var hasAccess = await _context.ArtistTeamMembers
            .HasViewerAccess(request.ArtistId, userId)
            .AnyAsync(cancellationToken);

        if (!hasAccess) throw new ForbiddenException("No access to view this artist's statistics.");

        string cacheKey = $"studio:artist:{request.ArtistId}:audience:{request.Days}";

        var cachedStats = await _cacheService.GetAsync<ArtistAudienceDashboardDto>(cacheKey, cancellationToken);
        if (cachedStats != null) return cachedStats;

        var trackIds = await _context.TrackArtists
            .AsNoTracking()
            .Where(ta => ta.ArtistId == request.ArtistId && !ta.Track.IsDeleted)
            .Select(ta => ta.TrackId)
            .ToListAsync(cancellationToken);

        if (!trackIds.Any()) 
        {
            return new ArtistAudienceDashboardDto(new(), new());
        }

        var minDate = _timeProvider.GetUtcNow().UtcDateTime.AddDays(-request.Days).Date;

        var geographyTask = _analyticsRepository.GetArtistGeographyAsync(trackIds, minDate, cancellationToken);
        var devicesTask = _analyticsRepository.GetArtistDevicesAsync(trackIds, minDate, cancellationToken);

        await Task.WhenAll(geographyTask, devicesTask);

        var result = new ArtistAudienceDashboardDto(
            TopCountries: geographyTask.Result,
            Devices: devicesTask.Result
        );

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30), cancellationToken);

        return result;
    }
}
