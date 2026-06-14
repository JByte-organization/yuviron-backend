using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Webhooks.Commands.MarkSubscriptionPaymentFailed;

public sealed class MarkSubscriptionPaymentFailedHandler : IRequestHandler<MarkSubscriptionPaymentFailedCommand, Unit>
{
    private readonly IMonetizationContext _monetizationContext;
    private readonly TimeProvider _timeProvider;
    private readonly IEventBus _eventBus;

    public MarkSubscriptionPaymentFailedHandler(
        IMonetizationContext monetizationContext,
        TimeProvider timeProvider,
        IEventBus eventBus)
    {
        _monetizationContext = monetizationContext;
        _timeProvider = timeProvider;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(MarkSubscriptionPaymentFailedCommand request, CancellationToken cancellationToken)
    {
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var userSubscription = await _monetizationContext.Subscriptions
            .Include(x => x.Plan)
            .FirstOrDefaultAsync(x => x.StripeSubscriptionId == request.StripeSubscriptionId, cancellationToken);

        if (userSubscription != null)
        {
            if (userSubscription.Status != SubscriptionStatus.Cancelled && userSubscription.Status != SubscriptionStatus.Expired)
            {
                userSubscription.MarkAsPastDue(utcNow);
                await _monetizationContext.SaveChangesAsync(cancellationToken);

                await _eventBus.PublishAsync(
                    new SubscriptionPaymentFailedEvent(
                        userSubscription.UserId,
                        userSubscription.PlanId,
                        userSubscription.Plan.Name,
                        request.StripeSubscriptionId,
                        request.FailureReason),
                    cancellationToken);
            }

            return Unit.Value;
        }

        var artistSubscription = await _monetizationContext.ArtistSubscriptions
            .Include(x => x.Plan)
            .FirstOrDefaultAsync(x => x.StripeSubscriptionId == request.StripeSubscriptionId, cancellationToken);

        if (artistSubscription != null)
        {
            if (artistSubscription.Status != SubscriptionStatus.Cancelled && artistSubscription.Status != SubscriptionStatus.Expired)
            {
                artistSubscription.MarkAsPastDue(utcNow);
                await _monetizationContext.SaveChangesAsync(cancellationToken);

                await _eventBus.PublishAsync(
                    new ArtistSubscriptionPaymentFailedEvent(
                        artistSubscription.ArtistId,
                        artistSubscription.PayerUserId,
                        artistSubscription.PlanId,
                        artistSubscription.Plan.Name,
                        request.StripeSubscriptionId,
                        request.FailureReason),
                    cancellationToken);
            }
        }

        return Unit.Value;
    }
}
