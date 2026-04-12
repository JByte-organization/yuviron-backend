using System.Diagnostics;
using StackExchange.Redis;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Services;

public sealed class AnalyticsService : IAnalyticsService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(
        IConnectionMultiplexer redis, 
        IApplicationDbContext context, 
        TimeProvider timeProvider, 
        ILogger<AnalyticsService> logger)
    {
        _redis = redis;
        _context = context;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task RecordTrackPlayAsync(Guid trackId, Guid artistId, CancellationToken ct = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var batch = db.CreateBatch();
            
            _ = batch.StringIncrementAsync($"track:{trackId}:plays");
            _ = batch.StringIncrementAsync($"artist:{artistId}:plays");
            _ = batch.SetAddAsync("dirty_counters:tracks", trackId.ToString());
            _ = batch.SetAddAsync("dirty_counters:artists", artistId.ToString());
            
            batch.Execute(); 
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis недоступен, пишем прослушивание в Outbox для {TrackId}", trackId);

            var fallbackEvent = new TrackPlayedFallbackEvent(trackId, artistId);
            
            var traceId = Activity.Current?.Id; 
            
            var message = OutboxMessage.Create(
                typeof(TrackPlayedFallbackEvent).AssemblyQualifiedName!,
                JsonSerializer.Serialize(fallbackEvent),
                _timeProvider.GetUtcNow().UtcDateTime,
                traceId);

            _context.OutboxMessages.Add(message);
            await _context.SaveChangesAsync(ct);
        }
    }
}