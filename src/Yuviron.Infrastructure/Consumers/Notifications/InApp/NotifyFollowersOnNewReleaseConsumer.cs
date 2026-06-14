using Yuviron.Infrastructure.Persistence;
using Yuviron.Application.Abstractions.Data.Contexts;
﻿using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers.Notifications.InApp;

public class NotifyFollowersOnNewReleaseConsumer : IConsumer<NewReleasePublishedEvent>
{
    private readonly AppDbContext _context;
    private readonly INotificationService _notificationService;

    public NotifyFollowersOnNewReleaseConsumer(AppDbContext context, INotificationService notificationService)
    {
        _context = context; _notificationService = notificationService;
    }

    public async Task Consume(ConsumeContext<NewReleasePublishedEvent> context)
    {
        var msg = context.Message;
        var followerIds = await _context.UserFollowArtists.AsNoTracking().Where(f => f.ArtistId == msg.ArtistId && f.NotifyNewReleases).Select(f => f.UserId).ToListAsync(context.CancellationToken);

        if (!followerIds.Any()) return; 

        await _notificationService.SendToUsersAsync(followerIds, NotificationCategory.Music, "new_release", 
            $"Новий реліз від {msg.ArtistName}! 🎵", $"{msg.ArtistName} щойно випустив «{msg.Title}».", msg.EntityType, msg.EntityId, context.CancellationToken);
    }
}
