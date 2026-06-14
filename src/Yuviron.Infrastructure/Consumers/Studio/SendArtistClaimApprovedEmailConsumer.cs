using Yuviron.Infrastructure.Persistence;
using Yuviron.Application.Abstractions.Data.Contexts;
﻿using MassTransit;
using Microsoft.Extensions.Logging;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Events;
using Yuviron.Application.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Yuviron.Infrastructure.Consumers;

public record ArtistClaimApprovedModel(string ArtistName, string StudioLink);

public class SendArtistClaimApprovedEmailConsumer : IConsumer<ArtistClaimApprovedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ITemplateService _templateService;
    private readonly FrontendOptions _frontendOptions;
    private readonly AppDbContext _context;
    private readonly ILogger<SendArtistClaimApprovedEmailConsumer> _logger;

    public SendArtistClaimApprovedEmailConsumer(
        IEmailService emailService,
        ITemplateService templateService,
        IOptions<FrontendOptions> frontendOptions,
        AppDbContext context,
        ILogger<SendArtistClaimApprovedEmailConsumer> logger)
    {
        _emailService = emailService;
        _templateService = templateService;
        _frontendOptions = frontendOptions.Value;
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ArtistClaimApprovedEvent> context)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == context.Message.UserId, context.CancellationToken);

        if (user == null) return;

        var frontendUrl = _frontendOptions.BaseUrl.TrimEnd('/');
        var studioLink = $"{frontendUrl}/studio/artists/{context.Message.ArtistId}";

        var htmlBody = await _templateService.RenderTemplateAsync("ArtistClaimApproved", 
            new ArtistClaimApprovedModel(context.Message.ArtistName, studioLink));

        await _emailService.SendEmailAsync(user.Email, "Вашу заявку на артиста схвалено! - Yuviron", htmlBody, context.CancellationToken);
        
        _logger.LogInformation("Artist claim approval email sent to {Email} for artist {ArtistName}.", user.Email, context.Message.ArtistName);
    }
}
