using Yuviron.Infrastructure.Persistence;
using Yuviron.Application.Abstractions.Data.Contexts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers.Notifications.InApp;

public sealed class NotifyTeamOnTrackLyricsUpdatedConsumer : IConsumer<TrackLyricsUpdatedEvent>
{
    private readonly AppDbContext _context;
    private readonly INotificationService _notificationService;

    public NotifyTeamOnTrackLyricsUpdatedConsumer(AppDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task Consume(ConsumeContext<TrackLyricsUpdatedEvent> context)
    {
        var teamIds = await _context.ArtistTeamMembers
            .AsNoTracking()
            .Where(tm => tm.ArtistId == context.Message.ArtistId && tm.UserId != context.Message.UpdatedByUserId)
            .Select(tm => tm.UserId)
            .ToListAsync(context.CancellationToken);

        if (!teamIds.Any()) return;

        await _notificationService.SendToUsersAsync(
            teamIds,
            NotificationCategory.System,
            "track_lyrics_updated",
            "Лірику оновлено",
            $"Текст пісні «{context.Message.TrackTitle}» був оновлений іншим учасником команди.",
            NotificationEntityType.Track,
            context.Message.TrackId,
            context.CancellationToken);
    }
}
