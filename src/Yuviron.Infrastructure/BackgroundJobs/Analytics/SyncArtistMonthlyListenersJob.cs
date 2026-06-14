using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Yuviron.Infrastructure.Persistence;
using Yuviron.Application.Abstractions.Analytics;

namespace Yuviron.Infrastructure.BackgroundJobs;

public sealed class SyncArtistMonthlyListenersJob : BackgroundService
{
    private static readonly TimeSpan SyncInterval = TimeSpan.FromHours(24);
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SyncArtistMonthlyListenersJob> _logger;

    public SyncArtistMonthlyListenersJob(IServiceProvider serviceProvider, ILogger<SyncArtistMonthlyListenersJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(SyncInterval);
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await SyncAsync(stoppingToken); }
            catch (Exception ex) { _logger.LogError(ex, "Critical error during monthly listeners sync."); }
            
            try { await timer.WaitForNextTickAsync(stoppingToken); }
            catch (OperationCanceledException) { break; }
        }
    }

    private async Task SyncAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();
        var analyticsRepository = scope.ServiceProvider.GetRequiredService<IAnalyticsRepository>();

        var fromDate = timeProvider.GetUtcNow().UtcDateTime.AddDays(-30);

        _logger.LogInformation("Starting Monthly Listeners computation (using ClickHouse)...");

        // 1. Get unique listeners per track from ClickHouse (Very fast)
        var trackListeners = await analyticsRepository.GetTracksUniqueListenersAsync(fromDate, cancellationToken);

        if (!trackListeners.Any())
        {
            _logger.LogInformation("No listening events found in ClickHouse for the last 30 days.");
            return;
        }

        // 2. Get Track -> Artist mapping from MySQL
        var trackIds = trackListeners.Keys.ToList();
        var trackToArtists = await context.TrackArtists
            .AsNoTracking()
            .Where(ta => trackIds.Contains(ta.TrackId))
            .Select(ta => new { ta.TrackId, ta.ArtistId })
            .ToListAsync(cancellationToken);

        // 3. Aggregate listeners per artist in memory
        var artistStats = new Dictionary<Guid, int>();
        foreach (var mapping in trackToArtists)
        {
            if (trackListeners.TryGetValue(mapping.TrackId, out var listeners))
            {
                if (!artistStats.ContainsKey(mapping.ArtistId))
                    artistStats[mapping.ArtistId] = 0;
                
                artistStats[mapping.ArtistId] += listeners;
            }
        }

        // 4. Update artists in MySQL
        var activeArtistIds = artistStats.Keys.ToList();
        
        // Also include artists who HAD listeners but now have 0
        var artistsToUpdate = await context.Artists
            .Where(a => activeArtistIds.Contains(a.Id) || a.MonthlyListenersCount > 0)
            .ToListAsync(cancellationToken);

        int updatedCount = 0;
        foreach (var artist in artistsToUpdate)
        {
            var newValue = artistStats.TryGetValue(artist.Id, out var count) ? count : 0;

            if (artist.MonthlyListenersCount != newValue)
            {
                artist.SetMonthlyListenersCount(newValue);
                updatedCount++;
            }
        }

        if (updatedCount > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation("Sync completed. {UpdatedCount} artists were updated.", updatedCount);
    }
}
