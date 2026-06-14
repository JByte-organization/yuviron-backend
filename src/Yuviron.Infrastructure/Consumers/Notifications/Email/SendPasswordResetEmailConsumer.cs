using MassTransit;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions.Authentication;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Events;
using Yuviron.Application.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Yuviron.Infrastructure.Consumers.Notifications.Email;

public record ForgotPasswordModel(string FirstName, string ResetLink);

public class SendPasswordResetEmailConsumer : IConsumer<ForgotPasswordRequestedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ITemplateService _templateService;
    private readonly IOtpService _otpService;
    private readonly FrontendOptions _frontendOptions;
    private readonly ILogger<SendPasswordResetEmailConsumer> _logger;

    public SendPasswordResetEmailConsumer(
        IEmailService emailService,
        ITemplateService templateService,
        IOtpService otpService,
        IOptions<FrontendOptions> frontendOptions,
        ILogger<SendPasswordResetEmailConsumer> logger)
    {
        _emailService = emailService;
        _templateService = templateService;
        _otpService = otpService;
        _frontendOptions = frontendOptions.Value;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ForgotPasswordRequestedEvent> context)
    {
        var token = context.Message.ResetToken;
        await _otpService.SavePasswordResetTokenAsync(token, context.Message.Email, TimeSpan.FromHours(2), context.CancellationToken);

        var frontendUrl = _frontendOptions.BaseUrl.TrimEnd('/');
        var resetLink = $"{frontendUrl}/reset-password?token={token}";

        var htmlBody = await _templateService.RenderTemplateAsync("ForgotPassword", new ForgotPasswordModel(context.Message.FirstName, resetLink));

        await _emailService.SendEmailAsync(context.Message.Email, "Скидання пароля - Yuviron", htmlBody, context.CancellationToken);
        
        _logger.LogInformation("Password reset link sent to {Email}.", context.Message.Email);
    }
}
