using MediatR;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Auth.EventHandlers;

public record WelcomeEmailModel(string FirstName, string AppUrl);

public sealed class SendWelcomeEmailEventHandler : INotificationHandler<UserRegisteredEvent>
{
    private readonly IEmailService _emailService;
    private readonly ITemplateService _templateService;
    private readonly ILogger<SendWelcomeEmailEventHandler> _logger;

    public SendWelcomeEmailEventHandler(
        IEmailService emailService,
        ITemplateService templateService,
        ILogger<SendWelcomeEmailEventHandler> logger)
    {
        _emailService = emailService;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {

        var templateModel = new WelcomeEmailModel(notification.FirstName, "https://dev.yuviron.com");

        var htmlBody = await _templateService.RenderTemplateAsync("WelcomeEmail", templateModel);

        await _emailService.SendEmailAsync(
            notification.Email,
            "Добро пожаловать в Yuviron! 🎵",
            htmlBody,
            cancellationToken
        );
        
        _logger.LogInformation("Приветственное письмо для {Email} успешно отправлено.", notification.Email);
    }
}