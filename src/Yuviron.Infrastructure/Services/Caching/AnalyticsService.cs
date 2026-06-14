using Yuviron.Infrastructure.Persistence;
using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
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
    private readonly AppDbContext _catalogContext;
    private readonly AppDbContext _systemContext;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(
        IConnectionMultiplexer redis, 
        AppDbContext catalogContext, AppDbContext systemContext, 
        TimeProvider timeProvider, 
        ILogger<AnalyticsService> logger)
    {
        _redis = redis;
        _catalogContext = catalogContext;
        _systemContext = systemContext;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<Guid> StartPlaySessionAsync(Guid trackId, Guid userId, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var sessionId = Guid.NewGuid();

        string sessionKey = $"play_session:{sessionId}:user:{userId}:track:{trackId}";
        
        long startTimeStamp = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
        
        await db.StringSetAsync(sessionKey, startTimeStamp.ToString(), TimeSpan.FromHours(1));

        return sessionId;
    }

    public async Task<int> CommitPlaySessionAsync(Guid sessionId, Guid trackId, Guid userId, CancellationToken ct = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            string sessionKey = $"play_session:{sessionId}:user:{userId}:track:{trackId}";

            var startTimeVal = await db.StringGetDeleteAsync(sessionKey);
            
            if (!startTimeVal.HasValue)
            {
                _logger.LogWarning("Anti-fraud: Сессия не найдена, истекла или уже обработана другим потоком. User: {UserId}", userId);
                return 0; 
            }

            long startTime = long.Parse(startTimeVal!);
            long currentTime = _timeProvider.GetUtcNow().ToUnixTimeMilliseconds();
            int msPlayed = (int)(currentTime - startTime);

            if (msPlayed < 30000) 
            {
                return msPlayed; 
            }
            
            var artistIds = await _catalogContext.TrackArtists
                .AsNoTracking()
                .Where(ta => ta.TrackId == trackId)
                .Select(ta => ta.ArtistId)
                .ToListAsync(ct);
            
            var batch = db.CreateBatch();
            var tasks = new List<Task>();
            
            tasks.Add(batch.StringIncrementAsync($"track:{trackId}:plays"));
            tasks.Add(batch.SetAddAsync("dirty_counters:tracks", trackId.ToString()));

            foreach (var artistId in artistIds)
            {
                tasks.Add(batch.StringIncrementAsync($"artist:{artistId}:plays"));
                tasks.Add(batch.SetAddAsync("dirty_counters:artists", artistId.ToString()));
            }
            
            batch.Execute();
            await Task.WhenAll(tasks);
            
            return msPlayed;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis недоступен, пишем прослушивание в Outbox для {TrackId}", trackId);
            
            var fallbackEvent = new TrackPlayedFallbackEvent(trackId, userId); 
            
            var traceId = Activity.Current?.Id; 
            var message = OutboxMessage.Create(
                typeof(TrackPlayedFallbackEvent).AssemblyQualifiedName!,
                JsonSerializer.Serialize(fallbackEvent),
                _timeProvider.GetUtcNow().UtcDateTime,
                traceId);

            _systemContext.Add(message);
            await _catalogContext.SaveChangesAsync(ct);
            return 0;
        }
    }
}