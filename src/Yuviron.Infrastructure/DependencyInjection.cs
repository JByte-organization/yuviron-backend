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
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Caching;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Security;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Configuration;
using Yuviron.Application.Features.Admin.Tracks.Consumers;
using Yuviron.Application.Policies;
using Yuviron.Infrastructure.Authentication;
using Yuviron.Infrastructure.BackgroundJobs;
using Yuviron.Infrastructure.Caching;
using Yuviron.Infrastructure.Configuration;
using Yuviron.Infrastructure.Consumers;
using Yuviron.Infrastructure.Identity;
using Yuviron.Infrastructure.Persistence;
using Yuviron.Infrastructure.Services;
using Yuviron.Infrastructure.Services.Audio;
using Yuviron.Infrastructure.HealthChecks;
using Yuviron.Infrastructure.Services.Security;

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
        services.AddHttpClient<IJamendoApiService, JamendoApiService>();
        services.AddSingleton<IStreamTokenService, StreamTokenService>();
        services.AddSingleton<UserSettingsPolicy>();

        // 5. UTILITIES
        services.AddSingleton(TimeProvider.System);
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddSingleton<ITemplateService, FluidTemplateService>();
        services.AddScoped<IOtpService, OtpService>();
        services.Configure<ArtistLimitsOptions>(configuration.GetSection(ArtistLimitsOptions.SectionName));

        // 6. HEALTH CHECKS
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" })
            .AddCheck<DatabaseHealthCheck>("mysql", tags: new[] { "ready" })
            .AddCheck<RedisHealthCheck>("redis", tags: new[] { "ready" })
            .AddRabbitMQ(
                async _ =>
                {
                    var host = configuration["RabbitMQ:Host"] ?? "127.0.0.1";
                    var port = configuration["RabbitMQ:Port"] ?? "5672";
                    var username = configuration["RabbitMQ:Username"] ?? "guest";
                    var password = configuration["RabbitMQ:Password"] ?? "guest";
                    var virtualHost = configuration["RabbitMQ:VirtualHost"] ?? "/";

                    var factory = new ConnectionFactory
                    {
                        HostName = host,
                        Port = int.Parse(port),
                        UserName = username,
                        Password = password,
                        VirtualHost = virtualHost
                    };

                    return await factory.CreateConnectionAsync();
                },
                "rabbitmq",
                null,
                new[] { "ready" }
            );

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
            
            
            x.AddConsumer<TrackPlayedFallbackConsumer>();

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
        services.AddHostedService<ProcessOutboxMessagesJob>();
        services.AddHostedService<TokenCleanupJob>();
        services.AddHostedService<TempFilesCleanupJob>();
        services.AddHostedService<SyncPlayCountsJob>();
        services.AddHostedService<SyncArtistMonthlyListenersJob>();

        return services;
    }
}
