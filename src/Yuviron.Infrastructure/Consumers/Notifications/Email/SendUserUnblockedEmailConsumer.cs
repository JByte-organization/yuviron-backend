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

namespace Yuviron.Infrastructure.Consumers.Notifications.Email;
public record UserUnblockedEmailModel(string FirstName);
public sealed class SendUserUnblockedEmailConsumer : IConsumer<UserUnblockedEvent>
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ITemplateService _templateService;
    private readonly ILogger<SendUserUnblockedEmailConsumer> _logger;

    public SendUserUnblockedEmailConsumer(
        AppDbContext context, 
        IEmailService emailService, 
        ITemplateService templateService,
        ILogger<SendUserUnblockedEmailConsumer> logger)
    {
        _context = context;
        _emailService = emailService;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserUnblockedEvent> context)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == context.Message.UserId, context.CancellationToken);

        if (user == null) return;

        var firstName = user.Profile?.FirstName ?? "Користувач";

        var htmlBody = await _templateService.RenderTemplateAsync("UserUnblocked", 
            new UserUnblockedEmailModel(firstName));

        await _emailService.SendEmailAsync(
            user.Email,
            "Блокування знято - Yuviron",
            htmlBody,
            context.CancellationToken);
            
        _logger.LogInformation("Unblocked email sent to {Email}", user.Email);
    }
}