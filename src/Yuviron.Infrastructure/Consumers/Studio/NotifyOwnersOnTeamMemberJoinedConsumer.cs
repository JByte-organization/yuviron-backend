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

public class NotifyOwnersOnTeamMemberJoinedConsumer : IConsumer<TeamMemberJoinedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public NotifyOwnersOnTeamMemberJoinedConsumer(IApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task Consume(ConsumeContext<TeamMemberJoinedEvent> context)
    {
        var ownerIds = await _context.ArtistTeamMembers
            .AsNoTracking()
            .Where(tm => tm.ArtistId == context.Message.ArtistId && tm.Role == ArtistTeamRole.Owner)
            .Select(tm => tm.UserId)
            .ToListAsync(context.CancellationToken);

        if (!ownerIds.Any()) return;

        await _notificationService.SendToUsersAsync(
            ownerIds, NotificationCategory.System, "team_joined",
            "В команді поповнення! 🤝",
            $"Користувач {context.Message.JoinedUserEmail} прийняв запрошення і приєднався до команди артиста {context.Message.ArtistName} як {context.Message.Role}.",
            NotificationEntityType.Artist, context.Message.ArtistId, context.CancellationToken);
    }
}