using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;

public record WelcomeEmailModel(string FirstName, string AppUrl);

public class SendWelcomeEmailConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly IEmailService _emailService;
    private readonly ITemplateService _templateService;
    private readonly ILogger<SendWelcomeEmailConsumer> _logger;

    public SendWelcomeEmailConsumer(
        IEmailService emailService,
        ITemplateService templateService,
        ILogger<SendWelcomeEmailConsumer> logger)
    {
        _emailService = emailService;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var templateModel = new WelcomeEmailModel(context.Message.FirstName, "https://dev.yuviron.com");

        var htmlBody = await _templateService.RenderTemplateAsync("WelcomeEmail", templateModel);

        await _emailService.SendEmailAsync(
            context.Message.Email,
            "Добро пожаловать в Yuviron! 🎵",
            htmlBody,
            context.CancellationToken
        );
        
        _logger.LogInformation("Приветственное письмо для {Email} успешно отправлено.", context.Message.Email);
    }
}