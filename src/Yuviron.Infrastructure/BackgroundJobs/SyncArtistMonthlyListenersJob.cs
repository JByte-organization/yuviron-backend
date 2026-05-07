using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Infrastructure.BackgroundJobs;

public sealed class SyncArtistMonthlyListenersJob : BackgroundService
{
    private const int BatchSize = 500;
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
        _logger.LogInformation(
            "The Artist Monthly Listeners Sync Service has started. Sync will occur every {Interval} hours.",
            SyncInterval.TotalHours);

        using var timer = new PeriodicTimer(SyncInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SyncAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error during artist monthly listeners sync.");
            }

            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task SyncAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var fromDate = utcNow.AddDays(-30);

        var listenerCountsQuery = context.ListeningEvents
            .AsNoTracking()
            .Where(le => le.UserId.HasValue &&
                         le.MsPlayed >= 30000 &&
                         le.PlayedAt >= fromDate)
            .SelectMany(le => le.Track.TrackArtists.Select(ta => new
            {
                ta.ArtistId,
                UserId = le.UserId!.Value
            }))
            .GroupBy(x => x.ArtistId)
            .Select(g => new
            {
                ArtistId = g.Key,
                MonthlyListenersCount = g.Select(x => x.UserId).Distinct().Count()
            });

        await context.Artists
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(a => a.MonthlyListenersCount, 0), cancellationToken);

        var page = 0;
        var updatedArtists = 0;

        while (true)
        {
            var batch = await listenerCountsQuery
                .OrderBy(x => x.ArtistId)
                .Skip(page * BatchSize)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);

            if (batch.Count == 0)
            {
                break;
            }

            var countsByArtistId = batch.ToDictionary(x => x.ArtistId, x => x.MonthlyListenersCount);
            var artistIds = countsByArtistId.Keys.ToList();

            var artists = await context.Artists
                .Where(a => artistIds.Contains(a.Id))
                .ToListAsync(cancellationToken);

            foreach (var artist in artists)
            {
                artist.SetMonthlyListenersCount(countsByArtistId[artist.Id]);
            }

            updatedArtists += artists.Count;

            await context.SaveChangesAsync(cancellationToken);
            context.ChangeTracker.Clear();

            page++;
        }

        _logger.LogInformation(
            "Artist monthly listeners sync completed. Updated {UpdatedArtists} artists for the last 30-day window.",
            updatedArtists);
    }
}
