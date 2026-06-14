using Yuviron.Infrastructure.Persistence;
using Yuviron.Application.Abstractions.Data.Contexts;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Infrastructure.Consumers.Bases;

namespace Yuviron.Infrastructure.Consumers;

public sealed class NotifyOwnersOnTrackPlayMilestoneConsumer : NotifyArtistOwnersConsumerBase<TrackPlayMilestoneReachedEvent>
{
    public NotifyOwnersOnTrackPlayMilestoneConsumer(AppDbContext context, INotificationService notificationService) : base(context, notificationService) { }

    protected override Task SendNotificationAsync(TrackPlayMilestoneReachedEvent msg, List<Guid> ownerIds, CancellationToken ct)
    {
        var title = msg.Milestone switch
        {
            10000 => "Трек подолав 10 000 прослуховувань",
            100000 => "Трек подолав 100 000 прослуховувань",
            1000000 => "Трек подолав 1 000 000 прослуховувань",
            _ => $"Трек подолав {msg.Milestone:N0} прослуховувань"
        };

        return NotificationService.SendToUsersAsync(
            ownerIds,
            NotificationCategory.Music,
            "track_play_milestone",
            title,
            $"Трек «{msg.TrackTitle}» уже має {msg.PlayCount:N0} прослуховувань.",
            NotificationEntityType.Track,
            msg.TrackId,
            ct);
    }
}
