using MassTransit;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Events;
using Yuviron.Application.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Yuviron.Infrastructure.Consumers;

public record TeamInviteModel(string ArtistName, string InviteLink);

public class SendTeamInviteEmailConsumer : IConsumer<SendTeamInviteEmailEvent>
{
    private readonly IEmailService _emailService;
    private readonly ITemplateService _templateService;
    private readonly FrontendOptions _frontendOptions;
    private readonly ILogger<SendTeamInviteEmailConsumer> _logger;

    public SendTeamInviteEmailConsumer(
        IEmailService emailService,
        ITemplateService templateService,
        IOptions<FrontendOptions> frontendOptions,
        ILogger<SendTeamInviteEmailConsumer> logger)
    {
        _emailService = emailService;
        _templateService = templateService;
        _frontendOptions = frontendOptions.Value;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SendTeamInviteEmailEvent> context)
    {
        var frontendUrl = _frontendOptions.BaseUrl.TrimEnd('/');
        var inviteLink = $"{frontendUrl}/studio/invites?code={context.Message.InviteToken}";

        var htmlBody = await _templateService.RenderTemplateAsync("TeamInvite", 
            new TeamInviteModel(context.Message.ArtistName, inviteLink));

        await _emailService.SendEmailAsync(context.Message.Email, $"Запрошення до команди {context.Message.ArtistName} - Yuviron", htmlBody, context.CancellationToken);
        
        _logger.LogInformation("Team invite email sent to {Email} for artist {ArtistName}.", context.Message.Email, context.Message.ArtistName);
    }
}
