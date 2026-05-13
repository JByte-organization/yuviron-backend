using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;

public record WelcomeEmailModel(string FirstName, string AppUrl);

public class SendWelcomeEmailConsumer : IConsumer<UserEmailConfirmedEvent> 
{
    private readonly IEmailService _emailService;
    private readonly ITemplateService _templateService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SendWelcomeEmailConsumer> _logger;

    public SendWelcomeEmailConsumer(
        IEmailService emailService, ITemplateService templateService,
        IConfiguration configuration, ILogger<SendWelcomeEmailConsumer> logger)
    {
        _emailService = emailService;
        _templateService = templateService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserEmailConfirmedEvent> context)
    {
        var frontendUrl = _configuration["FrontendUrl"] ?? "https://dev.yuviron.com";
        
        var htmlBody = await _templateService.RenderTemplateAsync("WelcomeEmail", new WelcomeEmailModel(context.Message.FirstName, frontendUrl));

        await _emailService.SendEmailAsync(context.Message.Email, "Ласкаво просимо до Yuviron! 🎵", htmlBody, context.CancellationToken);
        
        _logger.LogInformation("Welcome email sent to {Email}.", context.Message.Email);
    }
}