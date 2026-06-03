using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Yuviron.Infrastructure.Persistence;

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

        var fromDate = timeProvider.GetUtcNow().UtcDateTime.AddDays(-30);

        _logger.LogInformation("Starting Monthly Listeners computation...");

        var newStats = await context.ListeningEvents
            .AsNoTracking()
            .Where(le => le.UserId != null && le.MsPlayed >= 30000 && le.PlayedAt >= fromDate)
            .SelectMany(le => le.Track.TrackArtists.Select(ta => new { ta.ArtistId, UserId = le.UserId!.Value }))
            .GroupBy(x => x.ArtistId)
            .Select(g => new { ArtistId = g.Key, Count = g.Select(x => x.UserId).Distinct().Count() })
            .ToDictionaryAsync(x => x.ArtistId, x => x.Count, cancellationToken);

        var activeArtists = await context.Artists
            .Where(a => a.MonthlyListenersCount > 0 || newStats.Keys.Contains(a.Id))
            .ToListAsync(cancellationToken);

        int updatedCount = 0;

        foreach (var artist in activeArtists)
        {
            var newValue = newStats.TryGetValue(artist.Id, out var count) ? count : 0;

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