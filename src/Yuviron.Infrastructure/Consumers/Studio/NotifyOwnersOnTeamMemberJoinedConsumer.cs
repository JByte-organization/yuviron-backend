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

public class NotifyOwnersOnTeamMemberJoinedConsumer : NotifyArtistOwnersConsumerBase<TeamMemberJoinedEvent>
{
    public NotifyOwnersOnTeamMemberJoinedConsumer(AppDbContext context, INotificationService notificationService) : base(context, notificationService) { }

    protected override async Task SendNotificationAsync(TeamMemberJoinedEvent msg, List<Guid> ownerIds, CancellationToken ct) =>
        await NotificationService.SendToUsersAsync(ownerIds, NotificationCategory.System, "team_joined", 
            "В команді поповнення! 🤝", $"Користувач {msg.JoinedUserEmail} приєднався до команди як {msg.Role}.", NotificationEntityType.Artist, msg.ArtistId, ct);
}