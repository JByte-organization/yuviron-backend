using MassTransit;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Events;
using Yuviron.Application.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Yuviron.Infrastructure.Consumers.Notifications.Email;

public record WelcomeEmailModel(string FirstName, string LoginLink);

public class SendWelcomeEmailConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly IEmailService _emailService;
    private readonly ITemplateService _templateService;
    private readonly FrontendOptions _frontendOptions;
    private readonly ILogger<SendWelcomeEmailConsumer> _logger;

    public SendWelcomeEmailConsumer(
        IEmailService emailService,
        ITemplateService templateService,
        IOptions<FrontendOptions> frontendOptions,
        ILogger<SendWelcomeEmailConsumer> logger)
    {
        _emailService = emailService;
        _templateService = templateService;
        _frontendOptions = frontendOptions.Value;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var frontendUrl = _frontendOptions.BaseUrl.TrimEnd('/');
        var loginLink = $"{frontendUrl}/login";

        var htmlBody = await _templateService.RenderTemplateAsync("WelcomeEmail", new WelcomeEmailModel(context.Message.FirstName, loginLink));

        await _emailService.SendEmailAsync(context.Message.Email, "Ласкаво просимо до Yuviron!", htmlBody, context.CancellationToken);
        
        _logger.LogInformation("Welcome email sent to {Email}.", context.Message.Email);
    }
}
