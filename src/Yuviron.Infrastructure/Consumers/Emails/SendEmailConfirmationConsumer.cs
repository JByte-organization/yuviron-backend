using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;

public record ConfirmEmailModel(string FirstName, string ConfirmLink);

public class SendEmailConfirmationConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly IEmailService _emailService;
    private readonly ITemplateService _templateService;
    private readonly IOtpService _otpService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SendEmailConfirmationConsumer> _logger;

    public SendEmailConfirmationConsumer(
        IEmailService emailService, ITemplateService templateService,
        IOtpService otpService, IConfiguration configuration,
        ILogger<SendEmailConfirmationConsumer> logger)
    {
        _emailService = emailService;
        _templateService = templateService;
        _otpService = otpService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var token = Guid.NewGuid().ToString("N");
        await _otpService.SaveConfirmationTokenAsync(token, context.Message.Email, TimeSpan.FromHours(24), context.CancellationToken);

        var frontendUrl = _configuration["FrontendUrl"] ?? "https://dev.yuviron.com";
        var confirmLink = $"{frontendUrl}/confirm-email?token={token}";

        var htmlBody = await _templateService.RenderTemplateAsync("ConfirmEmail", new ConfirmEmailModel(context.Message.FirstName, confirmLink));

        await _emailService.SendEmailAsync(context.Message.Email, "Підтвердження реєстрації - Yuviron", htmlBody, context.CancellationToken);
        
        _logger.LogInformation("Email confirmation link sent to {Email}.", context.Message.Email);
    }
}