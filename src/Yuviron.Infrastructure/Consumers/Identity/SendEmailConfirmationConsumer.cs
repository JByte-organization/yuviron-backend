using MassTransit;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Events;
using Yuviron.Application.Configuration;
using Microsoft.Extensions.Options;

namespace Yuviron.Infrastructure.Consumers;

public record ConfirmEmailModel(string FirstName, string ConfirmLink);

public class SendEmailConfirmationConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly IEmailService _emailService;
    private readonly ITemplateService _templateService;
    private readonly IOtpService _otpService;
    private readonly FrontendOptions _frontendOptions;
    private readonly ILogger<SendEmailConfirmationConsumer> _logger;

    public SendEmailConfirmationConsumer(
        IEmailService emailService, ITemplateService templateService,
        IOtpService otpService, IOptions<FrontendOptions> frontendOptions,
        ILogger<SendEmailConfirmationConsumer> logger)
    {
        _emailService = emailService;
        _templateService = templateService;
        _otpService = otpService;
        _frontendOptions = frontendOptions.Value;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var token = Guid.NewGuid().ToString("N");
        await _otpService.SaveConfirmationTokenAsync(token, context.Message.Email, TimeSpan.FromHours(24), context.CancellationToken);

        var frontendUrl = _frontendOptions.BaseUrl.TrimEnd('/');
        var confirmLink = $"{frontendUrl}/confirm-email?token={token}";

        var htmlBody = await _templateService.RenderTemplateAsync("ConfirmEmail", new ConfirmEmailModel(context.Message.FirstName, confirmLink));

        await _emailService.SendEmailAsync(context.Message.Email, "Підтвердження реєстрації - Yuviron", htmlBody, context.CancellationToken);
        
        _logger.LogInformation("Email confirmation link sent to {Email}.", context.Message.Email);
    }
}
