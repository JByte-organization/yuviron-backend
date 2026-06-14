using Microsoft.Extensions.DependencyInjection;
using Yuviron.Infrastructure.BackgroundJobs;
using Yuviron.Infrastructure.BackgroundJobs.Cleanup;
using Yuviron.Infrastructure.MockData;
using Yuviron.Infrastructure.Analytics;

namespace Yuviron.Infrastructure.DependencyInjectionModules;

internal static class BackgroundServicesExtensions
{
    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<ClickHouseInitializer>();
        services.AddHostedService<ProcessOutboxMessagesJob>();
        services.AddHostedService<ExpiredDataCleanupJob>();
        services.AddHostedService<TempFilesCleanupJob>();
        services.AddHostedService<OrphanedDataCleanupJob>();
        services.AddHostedService<SyncPlayCountsJob>();
        services.AddHostedService<SyncArtistMonthlyListenersJob>();
        services.AddHostedService<DailyRoyaltyJob>();
        services.AddHostedService<BannerStartNotificationJob>();
        services.AddHostedService<TrackTrendDetectionJob>();
        services.AddHostedService<TrackTopChartNotificationJob>();

        return services;
    }
}
