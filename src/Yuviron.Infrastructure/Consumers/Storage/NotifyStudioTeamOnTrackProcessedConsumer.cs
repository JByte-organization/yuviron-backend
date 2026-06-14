using Yuviron.Infrastructure.Persistence;
using Yuviron.Application.Abstractions.Data.Contexts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Infrastructure.Consumers.Bases;

namespace Yuviron.Infrastructure.Consumers;

public class NotifyStudioTeamOnTrackProcessedConsumer : NotifyArtistTeamConsumerBase<TrackProcessingCompletedEvent>
{
    public NotifyStudioTeamOnTrackProcessedConsumer(AppDbContext context, INotificationService notificationService) : base(context, notificationService) { }

    protected override async Task SendNotificationAsync(TrackProcessingCompletedEvent msg, List<Guid> teamIds, CancellationToken ct) =>
        await NotificationService.SendToUsersAsync(teamIds, NotificationCategory.System, "track_processed", 
            "Трек готовий до публікації ✅", $"Ваш трек «{msg.TrackTitle}» успішно оброблено сервером.", NotificationEntityType.Track, msg.TrackId, ct);
}