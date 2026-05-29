using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Domain.Enums;

namespace Yuviron.Application.Features.Webhooks.Commands.RenewSubscription;

public sealed class RenewSubscriptionHandler : IRequestHandler<RenewSubscriptionCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public RenewSubscriptionHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
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
            listenerSub.Renew(listenerSub.EndAt.AddMonths(months), utcNow);
            
            await _context.SaveChangesAsync(cancellationToken);
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