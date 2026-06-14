using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetTrackRetention;
using Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetTrackPlaysOverTime;

// 1. ОБОВ'ЯЗКОВО ДОДАЙТЕ ЦЕЙ USING:
using Yuviron.Application.Features.StudioArtist.Analytics.Queries.GetArtistAudience;
using Yuviron.Application.Features.Admin.Ads.Queries.GetAdAnalytics;

namespace Yuviron.Application.Abstractions.Analytics;

public interface IAnalyticsRepository
{
    Task<List<TrackRetentionPointDto>> GetTrackRetentionAsync(Guid trackId, CancellationToken ct);
    Task<List<PlaysOverTimePointDto>> GetTrackPlaysOverTimeAsync(Guid trackId, DateTime minDate, CancellationToken ct);
    
    Task<List<PlaysOverTimePointDto>> GetArtistPlaysOverTimeAsync(IEnumerable<Guid> trackIds, DateTime minDate, CancellationToken ct);
    
    Task<List<AudienceGeographyDto>> GetArtistGeographyAsync(IEnumerable<Guid> trackIds, DateTime minDate, CancellationToken ct);
    Task<List<AudienceDeviceDto>> GetArtistDevicesAsync(IEnumerable<Guid> trackIds, DateTime minDate, CancellationToken ct);
    Task<List<TrackTrendCandidateDto>> GetTrendingTracksAsync(DateTime currentWindowFromUtc, DateTime previousWindowFromUtc, CancellationToken ct);
    Task<List<AdAnalyticsPointDto>> GetAdAnalyticsAsync(Guid adId, string interval, DateTime minDate, CancellationToken ct); Task<Dictionary<Guid, int>> GetTracksUniqueListenersAsync(DateTime minDate, CancellationToken ct);

    // --- Seeding Methods (for Mock Data) ---
    Task SeedListeningChunksAsync(IEnumerable<ListeningChunkSeedData> chunks, CancellationToken ct);
    Task SeedAdImpressionsAsync(IEnumerable<AdImpressionSeedData> impressions, CancellationToken ct);
}

public record ListeningChunkSeedData(
    Guid TrackId, 
    Guid? UserId, 
    DateTime PlayedAt, 
    ushort StartSecond, 
    ushort EndSecond, 
    string CountryCode, 
    string DeviceType);

public record AdImpressionSeedData(
    Guid AdId, 
    Guid UserId, 
    DateTime Timestamp, 
    string Context, 
    bool IsClicked);
