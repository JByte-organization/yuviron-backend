using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Payment;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Banners.Commands.RejectBannerRequest;

public sealed class RejectBannerRequestHandler : IRequestHandler<RejectBannerRequestCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly IPaymentService _paymentService;

    public RejectBannerRequestHandler(IApplicationDbContext context, TimeProvider timeProvider, IPaymentService paymentService)
    {
        _context = context; _timeProvider = timeProvider; _paymentService = paymentService;
    }

    public async Task<Unit> Handle(RejectBannerRequestCommand request, CancellationToken cancellationToken)
    {
        var bannerReq = await _context.BannerRequests
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
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}