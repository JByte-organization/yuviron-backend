using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Features.Finance.Commands.CalculateDailyRoyalties;

namespace Yuviron.Infrastructure.BackgroundJobs;

public sealed class DailyRoyaltyJob : BackgroundService
{
    private static readonly TimeSpan SyncInterval = TimeSpan.FromHours(24);
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DailyRoyaltyJob> _logger;

    public DailyRoyaltyJob(IServiceProvider serviceProvider, ILogger<DailyRoyaltyJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(SyncInterval);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try 
            { 
                await CalculateRoyaltiesAsync(stoppingToken); 
            }
            catch (Exception ex) 
            { 
                _logger.LogError(ex, "Critical error during daily royalty calculation."); 
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

    private async Task CalculateRoyaltiesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Waking up to calculate royalties!");

        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();

        var targetDate = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime.AddDays(-1));

        await mediator.Send(new CalculateDailyRoyaltiesCommand(targetDate), cancellationToken);
    }
}