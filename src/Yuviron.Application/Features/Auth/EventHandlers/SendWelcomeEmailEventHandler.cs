using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Auth.EventHandlers;

public record WelcomeEmailModel(string FirstName, string AppUrl);

public sealed class SendWelcomeEmailEventHandler : INotificationHandler<UserRegisteredEvent>
{
    private readonly IEmailJobQueue _emailJobQueue;
    private readonly ITemplateService _templateService;
    private readonly ILogger<SendWelcomeEmailEventHandler> _logger;

    public SendWelcomeEmailEventHandler(
        IEmailJobQueue emailJobQueue,
        ITemplateService templateService,
        ILogger<SendWelcomeEmailEventHandler> logger)
    {
        _emailJobQueue = emailJobQueue;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var templateModel = new WelcomeEmailModel(notification.FirstName, "https://dev.yuviron.com");

            var htmlBody = await _templateService.RenderTemplateAsync("WelcomeEmail", templateModel);

            var emailJob = new EmailJob(
                To: notification.Email,
                Subject: "Добро пожаловать в Yuviron! 🎵",
                Body: htmlBody
            );

            // Мгновенно кидаем в фоновую очередь (которую мы сделали ранее)
            await _emailJobQueue.EnqueueEmailAsync(emailJob);
            
            _logger.LogInformation("Приветственное письмо для {Email} успешно передано в очередь отправки.", notification.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка генерации приветственного письма для {Email}", notification.Email);
        }
    }
}