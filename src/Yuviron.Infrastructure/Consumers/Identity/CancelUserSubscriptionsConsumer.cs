using Yuviron.Infrastructure.Persistence;
using Yuviron.Application.Abstractions.Data.Contexts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Infrastructure.Consumers;

public class CancelUserSubscriptionsConsumer : IConsumer<UserDeletedEvent>
{
    private readonly AppDbContext _context;
    private readonly TimeProvider _timeProvider;

    public CancelUserSubscriptionsConsumer(AppDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task Consume(ConsumeContext<UserDeletedEvent> context)
    {
        var activeSubscriptions = await _context.Subscriptions
            .Where(s => s.UserId == context.Message.UserId && s.Status == SubscriptionStatus.Active)
            .ToListAsync(context.CancellationToken);

        if (!activeSubscriptions.Any()) return; 

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        foreach (var subscription in activeSubscriptions)
        {
            subscription.Cancel(utcNow, immediate: true); 
        }
        await _context.SaveChangesAsync(context.CancellationToken);
    }
}