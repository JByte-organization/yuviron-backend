using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;

namespace Yuviron.Infrastructure.BackgroundJobs;

public class TokenCleanupJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TokenCleanupJob> _logger;
    
    // Запускаем очистку раз в сутки
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24);

    public TokenCleanupJob(IServiceProvider serviceProvider, ILogger<TokenCleanupJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Token Cleanup Service запущен. Очистка будет происходить каждые {Interval} часов.", _checkInterval.TotalHours);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupTokensAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Критическая ошибка во время фоновой очистки токенов.");
            }

            // Ждем сутки до следующего запуска
            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CleanupTokensAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var sevenDaysAgo = utcNow.AddDays(-7);

        var deletedCount = await context.RefreshTokens
            .Where(t => t.ExpiresAt < utcNow || 
                        (t.RevokedAt != null && t.RevokedAt < sevenDaysAgo))
            .ExecuteDeleteAsync(stoppingToken);

        if (deletedCount > 0)
        {
            _logger.LogInformation("🧹 Уборка завершена: удалено {Count} протухших/старых токенов из БД.", deletedCount);
        }
    }
}