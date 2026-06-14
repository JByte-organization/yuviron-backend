using Yuviron.Infrastructure.Persistence;
using Yuviron.Application.Abstractions.Data.Contexts;
using System;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers.Notifications.InApp;

public sealed class NotifyUserOnModeratedPlaylistDeletedConsumer : IConsumer<ModeratedPlaylistDeletedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly AppDbContext _context;
    private readonly TimeProvider _timeProvider;

    public NotifyUserOnModeratedPlaylistDeletedConsumer(
        INotificationService notificationService,
        AppDbContext context,
        TimeProvider timeProvider)
    {
        _notificationService = notificationService;
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task Consume(ConsumeContext<ModeratedPlaylistDeletedEvent> context)
    {
        var msg = context.Message;
        
        if (msg.UserId.HasValue)
        {
            await _notificationService.SendToUserAsync(
                msg.UserId.Value,
                NotificationCategory.System,
                "playlist_deleted_by_admin",
                "Плейлист видалено",
                $"Ваш плейлист «{msg.PlaylistTitle}» було видалено адміністрацією.",
                NotificationEntityType.Playlist,
                msg.PlaylistId,
                context.CancellationToken);
        }

        // Автозакриття скарг
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        var complaints = await _context.Complaints
            .Where(c => c.TargetType == ComplaintTargetType.Playlist 
                     && c.TargetId == msg.PlaylistId 
                     && c.Status == ComplaintStatus.New)
            .ToListAsync(context.CancellationToken);

        bool hasChanges = false;
        foreach (var complaint in complaints)
        {
            complaint.MarkAsInReview(Guid.Empty, utcNow);
            complaint.Approve(Guid.Empty, "Автоматично підтверджено: плейлист видалено модератором.", utcNow);
            hasChanges = true;
        }

        var counter = await _context.ComplaintCounters
            .FirstOrDefaultAsync(cc => cc.TargetId == msg.PlaylistId, context.CancellationToken);
            
        if (counter != null && counter.CountOpen > 0)
        {
            counter.ResolveOpen(utcNow);
            hasChanges = true;
        }

        if (hasChanges) await _context.SaveChangesAsync(context.CancellationToken);
    }
}