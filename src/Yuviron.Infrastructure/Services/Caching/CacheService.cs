using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Yuviron.Application.Abstractions.Caching;

namespace Yuviron.Infrastructure.Caching;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<CacheService> _logger;

    public CacheService(
        IDistributedCache distributedCache, 
        IConnectionMultiplexer redis,
        ILogger<CacheService> logger)
    {
        _distributedCache = distributedCache;
        _redis = redis;
        _logger = logger;
    }


    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var cachedData = await _distributedCache.GetStringAsync(key, cancellationToken);
            if (string.IsNullOrEmpty(cachedData)) return default;

            return JsonSerializer.Deserialize<T>(cachedData);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Redis is unavailable. Falling back to database for key: {Key}", key);
            return default; 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading from cache for key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            string cacheValue = JsonSerializer.Serialize(value);
            var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(10) };
            await _distributedCache.SetStringAsync(key, cacheValue, options, cancellationToken);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Redis is unavailable. Could not set cache for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _distributedCache.RemoveAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove cache for key: {Key}. Rethrowing to trigger Outbox retry.", key);
            throw; 
        }
    }


    public async Task<bool> SortedSetAddAsync(string key, string member, double score, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            return await db.SortedSetAddAsync(key, member, score);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Redis unavailable. Could not add {Member} to ZSet {Key}", member, key);
            return false;
        }
    }

    public async Task<bool> SortedSetRemoveAsync(string key, string member, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            return await db.SortedSetRemoveAsync(key, member);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Redis unavailable. Could not remove {Member} from ZSet {Key}", member, key);
            return false;
        }
    }

    public async Task<string[]> SortedSetRangeByRankAsync(string key, long start = 0, long stop = -1, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var values = await db.SortedSetRangeByRankAsync(key, start, stop);
            return values.Select(v => v.ToString()).ToArray();
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Redis unavailable. Could not get range for ZSet {Key}", key);
            return Array.Empty<string>();
        }
    }

    public async Task<long> SortedSetLengthAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            return await db.SortedSetLengthAsync(key);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Redis unavailable. Could not get length for ZSet {Key}", key);
            return 0;
        }
    }

    public async Task<double?> SortedSetScoreAsync(string key, string member, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            return await db.SortedSetScoreAsync(key, member);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Redis unavailable. Could not get score for {Member} in ZSet {Key}", member, key);
            return null;
        }
    }
    
    public async Task<bool> SetAddAsync(string key, string member, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            return await db.SetAddAsync(key, member);
        }
        catch (RedisConnectionException) { return false; }
    }

    public async Task<long> SortedSetAddManyAsync(string key, Dictionary<string, double> membersAndScores, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var entries = membersAndScores.Select(kvp => new SortedSetEntry(kvp.Key, kvp.Value)).ToArray();
            return await db.SortedSetAddAsync(key, entries);
        }
        catch (RedisConnectionException) { return 0; }
    }
    
    public async Task<Dictionary<string, double>> SortedSetRangeByRankWithScoresAsync(string key, long start = 0, long stop = -1, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var entries = await db.SortedSetRangeByRankWithScoresAsync(key, start, stop);
            return entries.ToDictionary(e => e.Element.ToString(), e => e.Score);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Redis unavailable. Could not get range with scores for ZSet {Key}", key);
            return new Dictionary<string, double>();
        }
    }
    
    public async Task<bool> SetContainsAsync(string key, string member, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            return await db.SetContainsAsync(key, member);
        }
        catch (RedisConnectionException) { return false; }
    }

    public async Task<bool> SetRemoveAsync(string key, string member, CancellationToken cancellationToken = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            return await db.SetRemoveAsync(key, member);
        }
        catch (RedisConnectionException) { return false; }
    }
}