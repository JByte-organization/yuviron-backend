using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Payment;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Banners.Commands.RejectBannerRequest;

public sealed class RejectBannerRequestHandler : IRequestHandler<RejectBannerRequestCommand, Unit>
{
    private readonly IContentContext _contentContext;
    private readonly TimeProvider _timeProvider;
    private readonly IPaymentService _paymentService;
    private readonly IEventBus _eventBus;

    public RejectBannerRequestHandler(
        IContentContext contentContext,
        TimeProvider timeProvider,
        IPaymentService paymentService,
        IEventBus eventBus)
    {
        _contentContext = contentContext;
        _timeProvider = timeProvider;
        _paymentService = paymentService;
        _eventBus = eventBus;
    }

    public async Task<Unit> Handle(RejectBannerRequestCommand request, CancellationToken cancellationToken)
    {
        var bannerReq = await _contentContext.BannerRequests
                            .FirstOrDefaultAsync(br => br.Id == request.RequestId, cancellationToken)
                        ?? throw new NotFoundException(nameof(BannerRequest), request.RequestId);

        if (bannerReq.Status != BannerRequestStatus.Pending)
            throw new InvalidOperationException("Only pending banner requests can be rejected.");

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        if (bannerReq.IsPaid && !string.IsNullOrEmpty(bannerReq.StripePaymentIntentId))
        {
            await _paymentService.RefundPaymentAsync(bannerReq.StripePaymentIntentId, cancellationToken);
        }

        bannerReq.Reject(request.Reason, utcNow);
        await _contentContext.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(
            new BannerRequestRejectedEvent(
                bannerReq.SubmittedByUserId,
                bannerReq.ArtistId,
                bannerReq.Title,
                request.Reason),
            cancellationToken);

        return Unit.Value;
    }
}
