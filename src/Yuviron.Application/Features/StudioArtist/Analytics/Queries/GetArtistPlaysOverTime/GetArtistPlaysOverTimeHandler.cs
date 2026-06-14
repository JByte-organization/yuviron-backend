using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Analytics;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Exceptions;
using Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetTrackPlaysOverTime;

namespace Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetArtistPlaysOverTime;

public sealed class GetArtistPlaysOverTimeHandler : IRequestHandler<GetArtistPlaysOverTimeQuery, List<PlaysOverTimePointDto>>
{
    private readonly ICatalogContext _catalogContext;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cacheService;
    private readonly TimeProvider _timeProvider;
    private readonly IAnalyticsRepository _analyticsRepository;

    public GetArtistPlaysOverTimeHandler(
        ICatalogContext catalogContext, 
        ICurrentUserService currentUser,
        ICacheService cacheService,
        TimeProvider timeProvider,
        IAnalyticsRepository analyticsRepository)
    {
        _catalogContext = catalogContext;
        _currentUser = currentUser;
        _cacheService = cacheService;
        _timeProvider = timeProvider;
        _analyticsRepository = analyticsRepository;
    }

    public async Task<List<PlaysOverTimePointDto>> Handle(GetArtistPlaysOverTimeQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var hasAccess = await _catalogContext.ArtistTeamMembers
            .HasViewerAccess(request.ArtistId, userId)
            .AnyAsync(cancellationToken);

        if (!hasAccess) throw new ForbiddenException("No access to view this artist's statistics.");

        var trackIds = await _catalogContext.TrackArtists
            .AsNoTracking()
            .Where(ta => ta.ArtistId == request.ArtistId && !ta.Track.IsDeleted)
            .Select(ta => ta.TrackId)
            .ToListAsync(cancellationToken);

        if (!trackIds.Any()) return new List<PlaysOverTimePointDto>();

        string cacheKey = $"studio:artist:{request.ArtistId}:plays-over-time:{request.Days}";

        var cachedStats = await _cacheService.GetAsync<List<PlaysOverTimePointDto>>(cacheKey, cancellationToken);
        if (cachedStats != null) return cachedStats;

        var minDate = _timeProvider.GetUtcNow().UtcDateTime.AddDays(-request.Days).Date;

        var result = await _analyticsRepository.GetArtistPlaysOverTimeAsync(trackIds, minDate, cancellationToken);

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(15), cancellationToken);

        return result;
    }
}
