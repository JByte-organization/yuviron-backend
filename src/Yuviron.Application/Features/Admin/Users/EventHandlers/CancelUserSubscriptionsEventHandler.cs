using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Admin.Users.EventHandlers;

public sealed class CancelUserSubscriptionsEventHandler : INotificationHandler<UserDeletedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public CancelUserSubscriptionsEventHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task Handle(UserDeletedEvent notification, CancellationToken cancellationToken)
    {
        var activeSubscriptions = await _context.Subscriptions
            .Where(s => s.UserId == notification.UserId && s.Status == SubscriptionStatus.Active)
            .ToListAsync(cancellationToken);

        if (!activeSubscriptions.Any()) return; 

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        foreach (var subscription in activeSubscriptions)
        {
            subscription.Cancel(utcNow, immediate: true); 
        }
        await _context.SaveChangesAsync(cancellationToken);
    }
}