using MassTransit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;

public class NotifyStudioTeamOnPlaylistAdditionConsumer : IConsumer<TrackAddedToEditorialPlaylistEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public NotifyStudioTeamOnPlaylistAdditionConsumer(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task Consume(ConsumeContext<TrackAddedToEditorialPlaylistEvent> context)
    {
        var teamIds = await _context.ArtistTeamMembers
            .AsNoTracking()
            .Where(tm => tm.ArtistId == context.Message.ArtistId)
            .Select(tm => tm.UserId)
            .ToListAsync(context.CancellationToken);

        if (!teamIds.Any()) return;

        await _notificationService.SendToUsersAsync(
            teamIds, NotificationCategory.Music, "editorial_playlist",
            "Успіх редакції! 🌟",
            $"Ваш трек «{context.Message.TrackTitle}» потрапив в офіційний плейлист Yuviron «{context.Message.PlaylistName}».",
            NotificationEntityType.Track, context.Message.TrackId, context.CancellationToken);
    }
}