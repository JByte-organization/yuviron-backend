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

public sealed class NotifyOwnersOnArtistFollowersMilestoneConsumer : NotifyArtistOwnersConsumerBase<ArtistFollowersMilestoneReachedEvent>
{
    public NotifyOwnersOnArtistFollowersMilestoneConsumer(AppDbContext context, INotificationService notificationService) : base(context, notificationService) { }

    protected override Task SendNotificationAsync(ArtistFollowersMilestoneReachedEvent msg, List<Guid> ownerIds, CancellationToken ct) =>
        NotificationService.SendToUsersAsync(
            ownerIds,
            NotificationCategory.Social,
            "artist_followers_milestone",
            $"Артист зібрав {msg.Milestone:N0} підписників",
            $"Профіль «{msg.ArtistName}» перетнув позначку {msg.FollowerCount:N0} підписників.",
            NotificationEntityType.Artist,
            msg.ArtistId,
            ct);
}
