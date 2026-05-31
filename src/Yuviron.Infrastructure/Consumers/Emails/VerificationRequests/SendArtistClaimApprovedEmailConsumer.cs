using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;

public record ArtistClaimApprovedEmailModel(string FirstName, string ArtistName, string StudioLink);

public class SendArtistClaimApprovedEmailConsumer : IConsumer<ArtistClaimApprovedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ITemplateService _templateService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SendArtistClaimApprovedEmailConsumer> _logger;

    public SendArtistClaimApprovedEmailConsumer(
        IApplicationDbContext context, 
        IEmailService emailService, 
        ITemplateService templateService, 
        IConfiguration configuration,
        ILogger<SendArtistClaimApprovedEmailConsumer> logger)
    {
        _context = context;
        _emailService = emailService;
        _templateService = templateService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ArtistClaimApprovedEvent> context)
    {
        var msg = context.Message;
        var user = await _context.Users.Include(u => u.Profile).FirstOrDefaultAsync(u => u.Id == msg.UserId);
        if (user == null) return;

        var frontendUrl = _configuration["FrontendUrl"] ?? "https://dev.yuviron.com";
        var htmlBody = await _templateService.RenderTemplateAsync("ArtistClaimApproved", 
            new ArtistClaimApprovedEmailModel(user.Profile?.FirstName ?? "Артист", msg.ArtistName, $"{frontendUrl}/studio"));

        await _emailService.SendEmailAsync(user.Email, "Ваш профіль артиста підтверджено! 🚀 - Yuviron", htmlBody, context.CancellationToken);
        _logger.LogInformation("Approval email sent to {Email}.", user.Email);
    }
}