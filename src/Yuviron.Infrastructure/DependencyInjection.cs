using System.Diagnostics;
using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RabbitMQ.Client;
using StackExchange.Redis;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Analytics;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Identity;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Payment;
using Yuviron.Application.Abstractions.Security;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Abstractions.MockData;
using Yuviron.Application.Configuration;
using Yuviron.Application.Features.Admin.Tracks.Consumers;
using Yuviron.Application.Policies;
using Yuviron.Infrastructure.Analytics;
using Yuviron.Infrastructure.Authentication;
using Yuviron.Infrastructure.BackgroundJobs;
using Yuviron.Infrastructure.Caching;
using Yuviron.Infrastructure.Configuration;
using Yuviron.Infrastructure.Consumers;
using Yuviron.Infrastructure.Consumers.Content;
using Yuviron.Infrastructure.Consumers.Analytics;
using Yuviron.Infrastructure.Consumers.Monetization;
using Yuviron.Infrastructure.Identity;
using Yuviron.Infrastructure.Persistence;
using Yuviron.Infrastructure.Services;
using Yuviron.Infrastructure.Services.Audio;
using Yuviron.Infrastructure.HealthChecks;
using Yuviron.Infrastructure.Services.Payment;
using Yuviron.Infrastructure.Services.Security;
using Yuviron.Infrastructure.MockData;

namespace Yuviron.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Missing connection string: ConnectionStrings:Default");
        }

        services.AddDbContext<AppDbContext>(options =>
        {
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 43));
            options.UseMySql(connectionString, serverVersion, builder =>
            {
                builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                builder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });
        });
        
        services.AddSingleton(new ActivitySource("Yuviron.Infrastructure"));
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());
        services.AddScoped<AppDbContextInitializer>();

        // 2. AUTHENTICATION AND JWT
        var jwtSettings = new JwtSettings();
        configuration.Bind(JwtSettings.SectionName, jwtSettings);
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
                };
                
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/app"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IIdentityManager, IdentityManager>();

        // 3. CACHING (Redis)
        var redisConnectionString = configuration.GetConnectionString("Redis");
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
        });
        
        services.AddSingleton<IConnectionMultiplexer>(_ => 
        {
            var options = ConfigurationOptions.Parse(redisConnectionString!);
            options.AbortOnConnectFail = false; 
            return ConnectionMultiplexer.Connect(options);
        });

        services.AddSingleton<ICacheService, CacheService>();

        // 4. MEDIA SERVICES
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IAudioMetadataService, AudioMetadataService>();
        services.AddScoped<IHlsTranscodingService, HlsTranscodingService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IAnalyticsRepository, ClickHouseAnalyticsRepository>();
        services.AddScoped<IPaymentService, StripePaymentService>();
        services.AddScoped<IStripeWebhookParser, StripeWebhookParser>();
        services.AddHttpClient<IJamendoApiService, JamendoApiService>();
        services.AddSingleton<IStreamTokenService, StreamTokenService>();
        var audioSettings = configuration.GetSection("AudioSettings").Get<AudioSettingsOptions>() 
                            ?? new AudioSettingsOptions();

        services.AddSingleton(audioSettings); 
        services.AddSingleton<UserSettingsPolicy>();

        services.Configure<AudioSettingsOptions>(configuration.GetSection("AudioSettings"));
        
        services.Configure<StorageOptions>(configuration.GetSection("Storage"));

        // 5. UTILITIES
        services.AddSingleton(TimeProvider.System);
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddSingleton<ITemplateService, FluidTemplateService>();
        services.AddScoped<IOtpService, OtpService>();
        services.Configure<ArtistLimitsOptions>(configuration.GetSection(ArtistLimitsOptions.SectionName));
        services.Configure<CorsSettingsOptions>(configuration.GetSection(CorsSettingsOptions.SectionName));
        services.Configure<AdSettingsOptions>(configuration.GetSection(AdSettingsOptions.SectionName));
        services.Configure<FileAccessOptions>(configuration.GetSection(FileAccessOptions.SectionName));
        services.Configure<StripeOptions>(configuration.GetSection(StripeOptions.SectionName));
        services.Configure<MarketingOptions>(configuration.GetSection(MarketingOptions.SectionName));
        services.Configure<FrontendOptions>(configuration.GetSection(FrontendOptions.SectionName));
        services.AddScoped<IEventBus, MassTransitEventBus>();
        services.AddScoped<IClientContextService, ClientContextService>();
        services.AddScoped<IUserDeviceTracker, UserDeviceTracker>();
        services.AddScoped<IMockDataService, MockDataService>();
        
        

        // Singleton connection reused by the health check — avoids opening a new TCP
        // connection on every /health/ready probe (would leak hundreds of connections/hour).
        services.AddSingleton<IConnection>(_ =>
        {
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:Host"] ?? "127.0.0.1",
                Port = int.TryParse(configuration["RabbitMQ:Port"], out var p) ? p : 5672,
                UserName = configuration["RabbitMQ:Username"] ?? "guest",
                Password = configuration["RabbitMQ:Password"] ?? "guest",
                VirtualHost = configuration["RabbitMQ:VirtualHost"] ?? "/",
                AutomaticRecoveryEnabled = true,
            };
            return factory.CreateConnectionAsync("health-check").GetAwaiter().GetResult();
        });

        // 6. HEALTH CHECKS
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
            .AddCheck<DatabaseHealthCheck>("mysql", tags: new[] { "ready" })
            .AddCheck<RedisHealthCheck>("redis", tags: new[] { "ready" })
            .AddCheck<RabbitMqHealthCheck>("rabbitmq", null, new[] { "ready" });

        return services;
    }

    public static IServiceCollection AddApiBackgroundServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumer<MoveTempFileConsumer>();
            x.AddConsumer<DeleteFileConsumer>();
            x.AddConsumer<DeleteDirectoryConsumer>();
            
            x.AddConsumer<AlbumDeletedConsumer>();
            x.AddConsumer<HideArtistAlbumsConsumer>();
            x.AddConsumer<RemoveTrackFromPlaylistsConsumer>();
            x.AddConsumer<TrackDeletedConsumer>();
            
            x.AddConsumer<CancelUserSubscriptionsConsumer>();
            x.AddConsumer<ClearUserProfileConsumer>();
            x.AddConsumer<UserPermissionsChangedConsumer>();
            x.AddConsumer<RevokeTokensOnPasswordChangedConsumer>();
            x.AddConsumer<SendWelcomeEmailConsumer>();
            x.AddConsumer<SendEmailConfirmationConsumer>();
            x.AddConsumer<SendPasswordResetEmailConsumer>();
            x.AddConsumer<TrackSuccessfullyPlayedConsumer>();
            x.AddConsumer<TrackChunksListenedConsumer>();
            x.AddConsumer<AdImpressionRecordedConsumer>();
            
            x.AddConsumer<TrackPlayedFallbackConsumer>();
            
            x.AddConsumer<CreateArtistClaimApprovedNotificationConsumer>();
            x.AddConsumer<CreateArtistClaimRejectedNotificationConsumer>();
            x.AddConsumer<SendArtistClaimApprovedEmailConsumer>();
            x.AddConsumer<SendArtistClaimRejectedEmailConsumer>();
            x.AddConsumer<SendTeamInviteEmailConsumer>();
            x.AddConsumer<NotifyFollowersOnNewReleaseConsumer>();
            
            x.AddConsumer<NotifyOwnersOnPayoutApprovedConsumer>();
            x.AddConsumer<NotifyOwnersOnPayoutRejectedConsumer>();
            x.AddConsumer<NotifyOwnersOnPayoutRequestedConsumer>();
            x.AddConsumer<NotifyOwnersOnPayoutSettingsChangedConsumer>();
            x.AddConsumer<NotifyOwnersOnArtistSubscriptionActivatedConsumer>();
            x.AddConsumer<NotifyOwnersOnArtistSubscriptionCanceledConsumer>();
            x.AddConsumer<NotifyOwnersOnArtistFollowersMilestoneConsumer>();
            x.AddConsumer<NotifyOwnersOnTrackPlayMilestoneConsumer>();
            x.AddConsumer<NotifyUserOnPlaylistFavoritedConsumer>();
            
            x.AddConsumer<NotifyOwnersOnFirstRoyaltiesConsumer>();
            
            x.AddConsumer<NotifyStudioTeamOnTrackProcessedConsumer>();
            x.AddConsumer<NotifyStudioTeamOnPlaylistAdditionConsumer>();
            x.AddConsumer<NotifyTeamOnTrackLyricsUpdatedConsumer>();
            x.AddConsumer<NotifyTeamOnStudioPlaylistTrackChangedConsumer>();
            x.AddConsumer<NotifyOwnersOnTeamMemberJoinedConsumer>();
            x.AddConsumer<NotifyUserOnTeamMemberRemovedConsumer>();
            x.AddConsumer<NotifyUserOnTeamRoleChangedConsumer>();
            x.AddConsumer<NotifyOwnersOnModeratedTrackDeletedConsumer>();
            x.AddConsumer<NotifyOwnersOnModeratedAlbumDeletedConsumer>();
            x.AddConsumer<NotifyUserOnModeratedPlaylistDeletedConsumer>();
            x.AddConsumer<NotifyUserOnBannerRequestApprovedConsumer>();
            x.AddConsumer<NotifyUserOnBannerRequestRejectedConsumer>();
            x.AddConsumer<NotifyUserOnBannerRequestPaidConsumer>();
            x.AddConsumer<NotifyUserOnSubscriptionActivatedConsumer>();
            x.AddConsumer<NotifyUserOnSubscriptionRenewedConsumer>();
            x.AddConsumer<NotifyUserOnSubscriptionCanceledConsumer>();
            x.AddConsumer<NotifyUserOnSubscriptionPaymentFailedConsumer>();
            x.AddConsumer<NotifyOwnersOnArtistSubscriptionPaymentFailedConsumer>();
            x.AddConsumer<NotifyUserOnComplaintApprovedConsumer>();
            x.AddConsumer<NotifyUserOnComplaintRejectedConsumer>();
            x.AddConsumer<NotifyOwnersOnBannerStartedConsumer>();
            x.AddConsumer<NotifyOwnersOnTrackTrendingConsumer>();
            x.AddConsumer<NotifyOwnersOnTrackEnteredTopChartConsumer>();
            x.AddConsumer<NotifyTeamOnTrackProcessingFailedConsumer>();
            x.AddConsumer<NotifyUserOnPasswordChangedConsumer>();
            x.AddConsumer<SendUserOnPasswordResetCompletedConsumer>();
            x.AddConsumer<SendUserBlockedEmailConsumer>();
            x.AddConsumer<SendUserUnblockedEmailConsumer>();
            x.AddConsumer<NotifyUserOnNewFollowerConsumer>();
            x.AddConsumer<NotifyUsersOnSecurityEventsConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration["RabbitMQ:Host"] ?? "127.0.0.1";
                var user = configuration["RabbitMQ:Username"] ?? "guest";
                var pass = configuration["RabbitMQ:Password"] ?? "guest";

                var vhost = configuration["RabbitMQ:VirtualHost"] ?? "/";

                cfg.Host(host, vhost, h => {
                    h.Username(user);
                    h.Password(pass);
                });
                
                cfg.ReceiveEndpoint("api_background_tasks", e =>
                {
                    e.ConfigureConsumers(context); 
                });
            });
        });
        

        // Background tasks that only the API runs
        services.AddHostedService<ClickHouseInitializer>();
        services.AddHostedService<ProcessOutboxMessagesJob>();
        services.AddHostedService<TokenCleanupJob>();
        services.AddHostedService<TempFilesCleanupJob>();
        services.AddHostedService<SyncPlayCountsJob>();
        services.AddHostedService<SyncArtistMonthlyListenersJob>();
        services.AddHostedService<DailyRoyaltyJob>();
        services.AddHostedService<BannerStartNotificationJob>();
        services.AddHostedService<TrackTrendDetectionJob>();
        services.AddHostedService<TrackTopChartNotificationJob>();

        return services;
    }
}

