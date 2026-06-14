using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Events;

namespace Yuviron.Application.Features.Webhooks.Commands.FulfillBannerPayment;

public sealed class FulfillBannerPaymentHandler : IRequestHandler<FulfillBannerPaymentCommand, Unit>
{
    private readonly IContentContext _contentContext;
    private readonly TimeProvider _timeProvider;
    private readonly IEventBus _eventBus;

    public FulfillBannerPaymentHandler(IContentContext contentContext, TimeProvider timeProvider, IEventBus eventBus)
    {
        _contentContext = contentContext;
        _timeProvider = timeProvider;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(FulfillBannerPaymentCommand request, CancellationToken cancellationToken)
    {
        var bannerReq = await _contentContext.BannerRequests
            .FirstOrDefaultAsync(br => br.Id == request.BannerRequestId, cancellationToken);

        if (bannerReq == null || bannerReq.IsPaid) return Unit.Value;

        bannerReq.MarkAsPaid(request.PaymentIntentId, _timeProvider.GetUtcNow().UtcDateTime);
        await _contentContext.SaveChangesAsync(cancellationToken);

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
