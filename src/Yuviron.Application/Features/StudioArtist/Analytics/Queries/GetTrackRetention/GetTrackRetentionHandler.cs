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

namespace Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetTrackRetention;

public sealed class GetTrackRetentionHandler : IRequestHandler<GetTrackRetentionQuery, List<TrackRetentionPointDto>>
{
    private readonly ICatalogContext _catalogContext;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cacheService;
    private readonly IAnalyticsRepository _analyticsRepository; 

    public GetTrackRetentionHandler(
        ICatalogContext catalogContext, 
        ICurrentUserService currentUser,
        ICacheService cacheService,
        IAnalyticsRepository analyticsRepository)
    {
        _catalogContext = catalogContext;
        _currentUser = currentUser;
        _cacheService = cacheService;
        _analyticsRepository = analyticsRepository;
    }

    public async Task<List<TrackRetentionPointDto>> Handle(GetTrackRetentionQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var hasAccess = await _catalogContext.ArtistTeamMembers
            .HasViewerAccess(request.ArtistId, userId)
            .AnyAsync(cancellationToken);

        if (!hasAccess) throw new ForbiddenException("No access to view this artist's statistics.");

        bool trackBelongsToArtist = await _catalogContext.TrackArtists
            .AnyAsync(ta => ta.ArtistId == request.ArtistId && ta.TrackId == request.TrackId, cancellationToken);
            
        if (!trackBelongsToArtist) throw new ForbiddenException("Track does not belong to this artist.");

        string cacheKey = $"studio:artist:{request.ArtistId}:track:{request.TrackId}:retention";

        var cachedStats = await _cacheService.GetAsync<List<TrackRetentionPointDto>>(cacheKey, cancellationToken);
        if (cachedStats != null) return cachedStats;

        var result = await _analyticsRepository.GetTrackRetentionAsync(request.TrackId, cancellationToken);

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(30), cancellationToken);

        return result;
    }
}
