using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Analytics;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetTrackPlaysOverTime;

public sealed class GetTrackPlaysOverTimeHandler : IRequestHandler<GetTrackPlaysOverTimeQuery, List<PlaysOverTimePointDto>>
{
    private readonly ICatalogContext _catalogContext;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cacheService;
    private readonly TimeProvider _timeProvider;
    private readonly IAnalyticsRepository _analyticsRepository; 

    public GetTrackPlaysOverTimeHandler(
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

    public async Task<List<PlaysOverTimePointDto>> Handle(GetTrackPlaysOverTimeQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var hasAccess = await _catalogContext.ArtistTeamMembers
            .HasViewerAccess(request.ArtistId, userId)
            .AnyAsync(cancellationToken);

        if (!hasAccess) throw new ForbiddenException("No access to view this artist's statistics.");

        bool trackBelongsToArtist = await _catalogContext.TrackArtists
            .AnyAsync(ta => ta.ArtistId == request.ArtistId && ta.TrackId == request.TrackId, cancellationToken);
            
        if (!trackBelongsToArtist) throw new ForbiddenException("Track does not belong to this artist.");

        string cacheKey = $"studio:artist:{request.ArtistId}:track:{request.TrackId}:playsovertime:{request.Days}";

        var cachedStats = await _cacheService.GetAsync<List<PlaysOverTimePointDto>>(cacheKey, cancellationToken);
        if (cachedStats != null) return cachedStats;

        var minDate = _timeProvider.GetUtcNow().UtcDateTime.AddDays(-request.Days).Date;

        var result = await _analyticsRepository.GetTrackPlaysOverTimeAsync(request.TrackId, minDate, cancellationToken);

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(15), cancellationToken);

        return result;
    }
}
