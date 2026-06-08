using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Data;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;
public record UserBlockedEmailModel(string FirstName, string Reason, string BlockedUntil);
public sealed class SendUserBlockedEmailConsumer : IConsumer<UserBlockedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ITemplateService _templateService;
    private readonly ILogger<SendUserBlockedEmailConsumer> _logger;

    public SendUserBlockedEmailConsumer(
        IApplicationDbContext context, 
        IEmailService emailService, 
        ITemplateService templateService,
        ILogger<SendUserBlockedEmailConsumer> logger)
    {
        _context = context;
        _emailService = emailService;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserBlockedEvent> context)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == context.Message.UserId, context.CancellationToken);

        if (user == null) return;

        var blockedUntil = context.Message.EndsAtUtc.HasValue
            ? $"до {context.Message.EndsAtUtc.Value:yyyy-MM-dd HH:mm} UTC."
            : "безстроково.";

        var reasonText = string.IsNullOrWhiteSpace(context.Message.ReasonText)
            ? context.Message.ReasonCode
            : context.Message.ReasonText;

        var firstName = user.Profile?.FirstName ?? "Користувач";

        var htmlBody = await _templateService.RenderTemplateAsync("UserBlocked", 
            new UserBlockedEmailModel(firstName, reasonText, blockedUntil));

        await _emailService.SendEmailAsync(
            user.Email,
            "Обліковий запис заблоковано - Yuviron",
            htmlBody,
            context.CancellationToken);
            
        _logger.LogInformation("Blocked email sent to {Email}", user.Email);
    }
}