using Yuviron.Infrastructure.Persistence;
using Yuviron.Application.Abstractions.Data.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
namespace Yuviron.Infrastructure.Consumers.Notifications.InApp;

public sealed class NotifyOwnersOnModeratedAlbumDeletedConsumer : NotifyArtistOwnersConsumerBase<ModeratedAlbumDeletedEvent>
{
    private readonly AppDbContext _context;
    private readonly TimeProvider _timeProvider;

    public NotifyOwnersOnModeratedAlbumDeletedConsumer(
        AppDbContext context, 
        INotificationService notificationService,
        TimeProvider timeProvider) : base(context, notificationService) 
    { 
        _context = context;
        _timeProvider = timeProvider;
    }

    protected override Task SendNotificationAsync(ModeratedAlbumDeletedEvent msg, List<Guid> ownerIds, CancellationToken ct) =>
        NotificationService.SendToUsersAsync(ownerIds, NotificationCategory.System, "moderated_album_deleted", "Альбом видалено модератором", $"Альбом «{msg.AlbumTitle}» був видалений модератором через порушення правил.", NotificationEntityType.Album, msg.AlbumId, ct);

    protected override async Task AfterNotificationSentAsync(ModeratedAlbumDeletedEvent msg, List<Guid> ownerIds, CancellationToken ct)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        var complaints = await _context.Complaints
            .Where(c => c.TargetType == ComplaintTargetType.Album && c.TargetId == msg.AlbumId && c.Status == ComplaintStatus.New)
            .ToListAsync(ct);

        var counter = await _context.ComplaintCounters
            .FirstOrDefaultAsync(cc => cc.TargetType == ComplaintTargetType.Album && cc.TargetId == msg.AlbumId, ct);

        foreach (var complaint in complaints)
        {
            complaint.MarkAsInReview(Guid.Empty, utcNow);
            complaint.Approve(Guid.Empty, "Автоматично підтверджено: альбом видалено модератором.", utcNow);
        }

        if (counter != null && counter.CountOpen > 0) counter.ResolveOpen(utcNow);

        await _context.SaveChangesAsync(ct);
    }
}