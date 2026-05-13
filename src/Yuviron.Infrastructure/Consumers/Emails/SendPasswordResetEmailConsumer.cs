using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;

public record PasswordResetEmailModel(string FirstName, string ResetLink);

public class SendPasswordResetEmailConsumer : IConsumer<ForgotPasswordRequestedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ITemplateService _templateService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SendPasswordResetEmailConsumer> _logger;

    public SendPasswordResetEmailConsumer(
        IEmailService emailService, ITemplateService templateService,
        IConfiguration configuration, ILogger<SendPasswordResetEmailConsumer> logger)
    {
        _emailService = emailService;
        _templateService = templateService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ForgotPasswordRequestedEvent> context)
    {
        var message = context.Message;
        var frontendUrl = _configuration["FrontendUrl"] ?? "https://dev.yuviron.com";
        var resetLink = $"{frontendUrl}/reset-password?token={message.ResetToken}";

        var htmlBody = await _templateService.RenderTemplateAsync("ForgotPassword", new PasswordResetEmailModel(message.FirstName, resetLink));

        await _emailService.SendEmailAsync(message.Email, "Скидання паролю - Yuviron 🎵", htmlBody, context.CancellationToken);
        
        _logger.LogInformation("Password reset email sent to {Email}.", message.Email);
    }
}