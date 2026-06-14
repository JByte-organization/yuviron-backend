using Yuviron.Infrastructure.Persistence;
using Yuviron.Application.Abstractions.Data.Contexts;
﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions;

namespace Yuviron.Infrastructure.BackgroundJobs;

/// <summary>
/// Fast cleanup for ephemeral expired data (Tokens, OTPs, etc).
/// Similar to how high-load systems (Spotify/Netflix) manage TTL-based data.
/// </summary>
public sealed class ExpiredDataCleanupJob : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(6); // More frequent than consistency cleanup
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExpiredDataCleanupJob> _logger;

    public ExpiredDataCleanupJob(IServiceProvider serviceProvider, ILogger<ExpiredDataCleanupJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Expired Data Cleanup Service started. Running every {Interval} hours.", Interval.TotalHours);
        
        using var timer = new PeriodicTimer(Interval);
        do
        {
            try { await CleanupAsync(stoppingToken); }
            catch (Exception ex) { _logger.LogError(ex, "Error during expired data cleanup."); }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task CleanupAsync(CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();
        
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var staleRevocationCutoff = utcNow.AddDays(-7);

        // 1. Unpaid Banner Requests (Older than 7 days)
        int bannersDeleted = await context.BannerRequests
            .Where(br => br.Status == Yuviron.Domain.Enums.BannerRequestStatus.AwaitingPayment && br.CreatedAt < utcNow.AddDays(-7))
            .ExecuteDeleteAsync(ct);

        // 2. Refresh Tokens (Expired or Revoked long ago)
        int tokensDeleted = await context.RefreshTokens
            .Where(t => t.ExpiresAt < utcNow || (t.RevokedAt != null && t.RevokedAt < staleRevocationCutoff))
            .ExecuteDeleteAsync(ct);

        // 3. Expired Banners (Deactivate)
        int bannersDeactivated = await context.Banners
            .Where(b => b.IsActive && b.EndsAtUtc != null && b.EndsAtUtc < utcNow)
            .ExecuteUpdateAsync(s => s.SetProperty(b => b.IsActive, false), ct);

        // Note: You can add other TTL data here, like expired OTPs or temp invite codes.
        
        if (tokensDeleted > 0 || bannersDeleted > 0 || bannersDeactivated > 0)
        {
            _logger.LogInformation("Cleanup: Removed {TokensCount} tokens, {BannersCount} unpaid requests, and deactivated {DeactivatedCount} expired banners.", 
                tokensDeleted, bannersDeleted, bannersDeactivated);
        }
    }
}
