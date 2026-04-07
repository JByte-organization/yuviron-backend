using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions.Messaging;

namespace Yuviron.Infrastructure.BackgroundJobs;

public class EmailBackgroundWorker : BackgroundService
{
    private readonly IEmailJobQueue _queue;
    private readonly IServiceProvider _serviceProvider; // Нужен для создания Scope
    private readonly ILogger<EmailBackgroundWorker> _logger;

    public EmailBackgroundWorker(
        IEmailJobQueue queue, 
        IServiceProvider serviceProvider, 
        ILogger<EmailBackgroundWorker> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Background Worker запущен.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Ждем появления письма в очереди
                var job = await _queue.DequeueEmailAsync(stoppingToken);

                _logger.LogInformation("Извлечено письмо для {To} из очереди.", job.To);

                // Т.к. BackgroundService — это Singleton, нам нужно создать Scope, 
                // чтобы достать IEmailService (который обычно Scoped/Transient)
                using var scope = _serviceProvider.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                await emailService.SendEmailAsync(job.To, job.Subject, job.Body, stoppingToken);

                _logger.LogInformation("Письмо для {To} успешно отправлено из фона.", job.To);
            }
            catch (OperationCanceledException) { /* Норма при выключении сервера */ }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при фоновой отправке письма.");
                // Тут можно добавить логику ретрая (повтора), если SMTP упал
            }
        }
    }
}