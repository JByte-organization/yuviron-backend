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
using Yuviron.Infrastructure.Consumers.Bases;

namespace Yuviron.Infrastructure.Consumers;

public sealed class NotifyOwnersOnModeratedTrackDeletedConsumer : NotifyArtistOwnersConsumerBase<ModeratedTrackDeletedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public NotifyOwnersOnModeratedTrackDeletedConsumer(
        IApplicationDbContext context, 
        INotificationService notificationService,
        TimeProvider timeProvider) : base(context, notificationService) 
    { 
        _context = context;
        _timeProvider = timeProvider;
    }

    protected override Task SendNotificationAsync(ModeratedTrackDeletedEvent msg, List<Guid> ownerIds, CancellationToken ct) =>
        NotificationService.SendToUsersAsync(
            ownerIds,
            NotificationCategory.System,
            "moderated_track_deleted",
            "Трек видалено модератором",
            $"Трек «{msg.TrackTitle}» був видалений модератором через порушення правил.",
            NotificationEntityType.Track,
            msg.TrackId,
            ct);

    protected override async Task AfterNotificationSentAsync(ModeratedTrackDeletedEvent msg, List<Guid> ownerIds, CancellationToken ct)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        var complaints = await _context.Complaints
            .Where(c => c.TargetType == ComplaintTargetType.Track 
                     && c.TargetId == msg.TrackId 
                     && c.Status == ComplaintStatus.New)
            .ToListAsync(ct);

        var counter = await _context.ComplaintCounters
            .FirstOrDefaultAsync(cc => cc.TargetType == ComplaintTargetType.Track 
                                    && cc.TargetId == msg.TrackId, ct);

        foreach (var complaint in complaints)
        {
            complaint.MarkAsInReview(Guid.Empty, utcNow);
            complaint.Approve(Guid.Empty, "Автоматично підтверджено: контент видалено модератором.", utcNow);
        }

        if (counter != null && counter.CountOpen > 0)
            counter.ResolveOpen(utcNow);

        if (complaints.Any() || counter != null)
            await _context.SaveChangesAsync(ct);
    }
}