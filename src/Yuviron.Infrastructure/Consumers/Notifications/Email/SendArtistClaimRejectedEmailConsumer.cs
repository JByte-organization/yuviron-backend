using Yuviron.Infrastructure.Persistence;
using Yuviron.Application.Abstractions.Data.Contexts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers.Notifications.Email;

public record ArtistClaimRejectedEmailModel(string FirstName, string ArtistName, string Reason);

public class SendArtistClaimRejectedEmailConsumer : IConsumer<ArtistClaimRejectedEvent>
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ITemplateService _templateService;
    private readonly ILogger<SendArtistClaimRejectedEmailConsumer> _logger;

    public SendArtistClaimRejectedEmailConsumer(
        AppDbContext context, 
        IEmailService emailService, 
        ITemplateService templateService, 
        ILogger<SendArtistClaimRejectedEmailConsumer> logger)
    {
        _context = context;
        _emailService = emailService;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ArtistClaimRejectedEvent> context)
    {
        var msg = context.Message;
        var user = await _context.Users.Include(u => u.Profile).FirstOrDefaultAsync(u => u.Id == msg.UserId);
        if (user == null) return;

        var reasonText = string.IsNullOrWhiteSpace(msg.AdminNote) 
            ? "Дані не відповідають правилам платформи або надано недостатньо доказів." : msg.AdminNote;

        var htmlBody = await _templateService.RenderTemplateAsync("ArtistClaimRejected", 
            new ArtistClaimRejectedEmailModel(user.Profile?.FirstName ?? "Артист", msg.ArtistName, reasonText));

        await _emailService.SendEmailAsync(user.Email, "Статус заявки на профіль артиста - Yuviron", htmlBody, context.CancellationToken);
        _logger.LogInformation("Rejection email sent to {Email}.", user.Email);
    }
}