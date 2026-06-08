using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Webhooks.Commands.RenewSubscription;

public sealed class RenewSubscriptionHandler : IRequestHandler<RenewSubscriptionCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IEventBus _eventBus;

    public RenewSubscriptionHandler(IApplicationDbContext context, TimeProvider timeProvider, IEventBus eventBus)
    {
        _context = context;
        _timeProvider = timeProvider;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(RenewSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var stripeSubId = request.StripeSubscriptionId;

        var listenerSub = await _context.Subscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubId, cancellationToken);

        if (listenerSub != null)
        {
            var months = listenerSub.Plan.Period == PlanPeriod.Month ? 1 : 12;
            var newEndAt = listenerSub.EndAt.AddMonths(months);
            listenerSub.Renew(newEndAt, utcNow);
            
            await _context.SaveChangesAsync(cancellationToken);

            await _eventBus.PublishAsync(
                new SubscriptionRenewedEvent(
                    listenerSub.UserId,
                    listenerSub.PlanId,
                    listenerSub.Plan.Name,
                    newEndAt),
                cancellationToken);
            return Unit.Value; 
        }

        var artistSub = await _context.ArtistSubscriptions
            .Include(s => s.Plan) 
            .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubId, cancellationToken);
            
        if (artistSub != null)
        {
            var months = artistSub.Plan.Period == PlanPeriod.Month ? 1 : 12;
            artistSub.Renew(artistSub.EndAt.AddMonths(months), utcNow);
            
            await _context.SaveChangesAsync(cancellationToken);
            return Unit.Value; 
        }

        return Unit.Value;
    }
}
