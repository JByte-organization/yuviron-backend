using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Events;
using Yuviron.Infrastructure.Persistence;

namespace Yuviron.Infrastructure.BackgroundJobs;

public sealed class BannerStartNotificationJob : BackgroundService
{
    private static readonly TimeSpan SyncInterval = TimeSpan.FromMinutes(15);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<BannerStartNotificationJob> _logger;

    public BannerStartNotificationJob(
        IServiceScopeFactory scopeFactory,
        TimeProvider timeProvider,
        ILogger<BannerStartNotificationJob> logger)
    {
        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task RunOnceAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var banners = await context.Banners
            .Where(b =>
                b.IsActive &&
                b.ArtistId.HasValue &&
                b.StartsAtUtc.HasValue &&
                b.StartsAtUtc <= utcNow &&
                b.StartNotificationSentAtUtc == null)
            .OrderBy(b => b.StartsAtUtc)
            .ToListAsync(cancellationToken);

        if (!banners.Any())
        {
            return;
        }

        foreach (var banner in banners)
        {
            if (!banner.ArtistId.HasValue)
            {
                continue;
            }

            await eventBus.PublishAsync(
                new BannerStartedEvent(banner.ArtistId.Value, banner.Id, banner.Title),
                cancellationToken);

            banner.MarkStartNotificationSent(utcNow);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(SyncInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnceAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing banner start notifications.");
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
}
