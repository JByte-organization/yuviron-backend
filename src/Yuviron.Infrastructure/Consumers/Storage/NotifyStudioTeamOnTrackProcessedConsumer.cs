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

public class NotifyStudioTeamOnTrackProcessedConsumer : IConsumer<TrackProcessingCompletedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public NotifyStudioTeamOnTrackProcessedConsumer(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task Consume(ConsumeContext<TrackProcessingCompletedEvent> context)
    {
        var teamIds = await _context.ArtistTeamMembers
            .AsNoTracking()
            .Where(tm => tm.ArtistId == context.Message.ArtistId)
            .Select(tm => tm.UserId)
            .ToListAsync(context.CancellationToken);

        if (!teamIds.Any()) return;

        await _notificationService.SendToUsersAsync(
            teamIds, NotificationCategory.System, "track_processed",
            "Трек готовий до публікації ✅",
            $"Ваш трек «{context.Message.TrackTitle}» успішно оброблено сервером (HLS конвертація завершена) і готовий до релізу!",
            NotificationEntityType.Track, context.Message.TrackId, context.CancellationToken);
    }
}