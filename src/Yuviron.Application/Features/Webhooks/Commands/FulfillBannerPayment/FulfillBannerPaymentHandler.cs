using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Webhooks.Commands.FulfillBannerPayment;

public sealed class FulfillBannerPaymentHandler : IRequestHandler<FulfillBannerPaymentCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IEventBus _eventBus;

    public FulfillBannerPaymentHandler(IApplicationDbContext context, TimeProvider timeProvider, IEventBus eventBus)
    {
        _context = context;
        _timeProvider = timeProvider;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(FulfillBannerPaymentCommand request, CancellationToken cancellationToken)
    {
        var bannerReq = await _context.BannerRequests
            .FirstOrDefaultAsync(br => br.Id == request.BannerRequestId, cancellationToken);

        if (bannerReq == null || bannerReq.IsPaid) return Unit.Value;

        bannerReq.MarkAsPaid(request.PaymentIntentId, _timeProvider.GetUtcNow().UtcDateTime);
        await _context.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(
            new BannerRequestPaidEvent(
                bannerReq.SubmittedByUserId,
                bannerReq.ArtistId,
                bannerReq.Title,
                request.PaymentIntentId),
            cancellationToken);

        return Unit.Value;
    }
}
