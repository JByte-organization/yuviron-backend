using Yuviron.Infrastructure.Persistence;
using Yuviron.Application.Abstractions.Data.Contexts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;

public sealed class NotifyTeamOnStudioPlaylistTrackChangedConsumer : IConsumer<StudioPlaylistTrackChangedEvent>
{
    private readonly AppDbContext _context;
    private readonly INotificationService _notificationService;

    public NotifyTeamOnStudioPlaylistTrackChangedConsumer(AppDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task Consume(ConsumeContext<StudioPlaylistTrackChangedEvent> context)
    {
        var teamIds = await _context.ArtistTeamMembers
            .AsNoTracking()
            .Where(tm => tm.ArtistId == context.Message.ArtistId && tm.UserId != context.Message.ChangedByUserId)
            .Select(tm => tm.UserId)
            .ToListAsync(context.CancellationToken);

        if (!teamIds.Any()) return;

        var (title, body) = context.Message.Action switch
        {
            "removed" => (
                "Трек видалено з плейлиста",
                $"У плейлисті «{context.Message.PlaylistName}» було видалено трек «{context.Message.TrackTitle}»."),
            _ => (
                "Плейлист оновлено",
                $"Порядок треків у плейлисті «{context.Message.PlaylistName}» було змінено.")
        };

        await _notificationService.SendToUsersAsync(
            teamIds,
            NotificationCategory.System,
            "studio_playlist_track_changed",
            title,
            body,
            NotificationEntityType.Playlist,
            context.Message.PlaylistId,
            context.CancellationToken);
    }
}
