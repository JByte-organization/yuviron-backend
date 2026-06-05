using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;

namespace Yuviron.Application.Features.Webhooks.Commands.FulfillBannerPayment;

public sealed class FulfillBannerPaymentHandler : IRequestHandler<FulfillBannerPaymentCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public FulfillBannerPaymentHandler(IApplicationDbContext context, TimeProvider timeProvider)
    {
        _context = context; _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(FulfillBannerPaymentCommand request, CancellationToken cancellationToken)
    {
        var bannerReq = await _context.BannerRequests
            .FirstOrDefaultAsync(br => br.Id == request.BannerRequestId, cancellationToken);

        if (bannerReq == null || bannerReq.IsPaid) return Unit.Value;

        bannerReq.MarkAsPaid(request.PaymentIntentId, _timeProvider.GetUtcNow().UtcDateTime);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}