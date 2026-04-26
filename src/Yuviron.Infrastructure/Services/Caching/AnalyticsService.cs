using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
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

    public async Task<Guid> StartPlaySessionAsync(Guid trackId, Guid userId, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var sessionId = Guid.NewGuid();

        string sessionKey = $"play_session:{sessionId}:user:{userId}:track:{trackId}";
        
        long startTimeStamp = _timeProvider.GetUtcNow().ToUnixTimeSeconds();
        
        await db.StringSetAsync(sessionKey, startTimeStamp.ToString(), TimeSpan.FromHours(1));

        return sessionId;
    }

    public async Task CommitPlaySessionAsync(Guid sessionId, Guid trackId, Guid artistId, Guid userId, CancellationToken ct = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            string sessionKey = $"play_session:{sessionId}:user:{userId}:track:{trackId}";

            var startTimeVal = await db.StringGetDeleteAsync(sessionKey);
            
            if (!startTimeVal.HasValue)
            {
                _logger.LogWarning("Anti-fraud: Сессия не найдена, истекла или уже обработана другим потоком. User: {UserId}", userId);
                return; 
            }

            long startTime = long.Parse(startTimeVal!);
            long currentTime = _timeProvider.GetUtcNow().ToUnixTimeSeconds();
            
            if (currentTime - startTime < 30)
            {
                _logger.LogWarning("Anti-fraud: Слишком быстро! Блокируем накрутку от User {UserId}", userId);
                return; 
            }

            var batch = db.CreateBatch();
            var t1 = batch.StringIncrementAsync($"track:{trackId}:plays");
            var t2 = batch.StringIncrementAsync($"artist:{artistId}:plays");
            var t3 = batch.SetAddAsync("dirty_counters:tracks", trackId.ToString());
            var t4 = batch.SetAddAsync("dirty_counters:artists", artistId.ToString());
            
            batch.Execute(); 
            
            await Task.WhenAll(t1, t2, t3, t4); 
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