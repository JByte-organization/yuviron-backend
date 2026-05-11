using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Yuviron.Application.Abstractions;

namespace Yuviron.Infrastructure.BackgroundJobs;

public class RestorePlaylistCacheJob : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RestorePlaylistCacheJob> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(15);

    public RestorePlaylistCacheJob(IConnectionMultiplexer redis, IServiceScopeFactory scopeFactory, ILogger<RestorePlaylistCacheJob> logger)
    {
        _redis = redis;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RestoreCachesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка восстановления кэша плейлистов");
            }
            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task RestoreCachesAsync(CancellationToken ct)
    {
        var db = _redis.GetDatabase();
        
        var playlistIdsRaw = await db.SetMembersAsync("missing_cache:playlists");
        if (playlistIdsRaw.Length == 0) return;

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        foreach (var idRaw in playlistIdsRaw)
        {
            var playlistId = Guid.Parse(idRaw.ToString());
            var redisKey = $"playlist:{playlistId}:tracks";

            var tracks = await context.PlaylistTracks
                .AsNoTracking()
                .Where(pt => pt.PlaylistId == playlistId)
                .Select(pt => new { pt.TrackId, pt.Position })
                .ToListAsync(ct);

            if (tracks.Any())
            {
                var entries = tracks.Select(t => new SortedSetEntry(t.TrackId.ToString(), t.Position)).ToArray();
                
                await db.KeyDeleteAsync(redisKey); 
                await db.SortedSetAddAsync(redisKey, entries);
            }

            await db.SetRemoveAsync("missing_cache:playlists", idRaw);
            _logger.LogInformation("Успешно восстановлен ZSet кэш для плейлиста {PlaylistId}", playlistId);
        }
    }
}