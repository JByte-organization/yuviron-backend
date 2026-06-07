using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;

public record TeamInviteEmailModel(string ArtistName, string RoleName, string InviteLink);

public class SendTeamInviteEmailConsumer : IConsumer<SendTeamInviteEmailEvent>
{
    private readonly IEmailService _emailService;
    private readonly ITemplateService _templateService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SendTeamInviteEmailConsumer> _logger;

    public SendTeamInviteEmailConsumer(
        IEmailService emailService, 
        ITemplateService templateService, 
        IConfiguration configuration,
        ILogger<SendTeamInviteEmailConsumer> logger)
    {
        _emailService = emailService;
        _templateService = templateService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SendTeamInviteEmailEvent> context)
    {
        var msg = context.Message;

        var frontendUrl = _configuration["FrontendUrl"] ?? "https://yuviron.com";
        
        var inviteLink = $"{frontendUrl}/studio/invite?token={msg.InviteToken}&artistId={msg.ArtistId}";

        var htmlBody = await _templateService.RenderTemplateAsync("TeamInvite", 
            new TeamInviteEmailModel(msg.ArtistName, msg.RoleName, inviteLink));

        await _emailService.SendEmailAsync(
            msg.Email, 
            $"Запрошення в команду артиста {msg.ArtistName} 🎵 - Yuviron", 
            htmlBody, 
            context.CancellationToken);
            
        _logger.LogInformation("Team invite email sent to {Email} for artist {ArtistName}.", msg.Email, msg.ArtistName);
    }
}