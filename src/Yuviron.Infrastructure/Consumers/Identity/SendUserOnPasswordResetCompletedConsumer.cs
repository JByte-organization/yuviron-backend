using Yuviron.Infrastructure.Persistence;
using Yuviron.Application.Abstractions.Data.Contexts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Data;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;
public record PasswordResetCompletedEmailModel(string FirstName);
public sealed class SendUserOnPasswordResetCompletedConsumer : IConsumer<UserPasswordResetCompletedEvent>
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ITemplateService _templateService;
    private readonly ILogger<SendUserOnPasswordResetCompletedConsumer> _logger;

    public SendUserOnPasswordResetCompletedConsumer(
        AppDbContext context, 
        IEmailService emailService, 
        ITemplateService templateService,
        ILogger<SendUserOnPasswordResetCompletedConsumer> logger)
    {
        _context = context;
        _emailService = emailService;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserPasswordResetCompletedEvent> context)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == context.Message.UserId, context.CancellationToken);

        if (user == null) return;

        var firstName = user.Profile?.FirstName ?? "Користувач";

        var htmlBody = await _templateService.RenderTemplateAsync("PasswordResetCompleted", 
            new PasswordResetCompletedEmailModel(firstName));

        await _emailService.SendEmailAsync(
            user.Email,
            "Відновлення пароля завершено - Yuviron",
            htmlBody,
            context.CancellationToken);
            
        _logger.LogInformation("Password reset completion email sent to {Email}", user.Email);
    }
}