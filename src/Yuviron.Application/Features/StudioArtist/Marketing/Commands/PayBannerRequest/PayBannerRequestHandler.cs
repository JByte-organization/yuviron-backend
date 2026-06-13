using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Payment;
using Yuviron.Application.Configuration;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;

namespace Yuviron.Application.Features.StudioArtist.Marketing.Commands.PayBannerRequest;

public sealed class PayBannerRequestHandler : IRequestHandler<PayBannerRequestCommand, PayBannerResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IPaymentService _paymentService;
    private readonly MarketingOptions _options;

    public PayBannerRequestHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IPaymentService paymentService,
        IOptions<MarketingOptions> options)
    {
        _context = context;
        _currentUser = currentUser;
        _paymentService = paymentService;
        _options = options.Value;
    }

    public async Task<PayBannerResponse> Handle(PayBannerRequestCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var user = await _context.Users.FirstAsync(u => u.Id == userId, cancellationToken);

        var hasPermission = await _context.ArtistTeamMembers
            .HasManagementAccess(request.ArtistId, userId)
            .AnyAsync(cancellationToken);

        if (!hasPermission) throw new ForbiddenException("No access to manage marketing for this artist.");

        var bannerRequest = await _context.BannerRequests
            .FirstOrDefaultAsync(br => br.Id == request.RequestId && br.ArtistId == request.ArtistId, cancellationToken);

        if (bannerRequest == null)
            throw new NotFoundException("BannerRequest", request.RequestId);

        if (bannerRequest.Status != BannerRequestStatus.AwaitingPayment)
            throw new InvalidOperationException("This banner request cannot be paid because it is not in AwaitingPayment status.");

        decimal totalPrice = bannerRequest.DurationDays * _options.BannerPricePerDay;

        var stripeSession = await _paymentService.CreateBannerCheckoutSessionAsync(
            user, bannerRequest, totalPrice, _options.BannerCurrency, request.SuccessUrl, request.CancelUrl, cancellationToken);

        bannerRequest.SetCheckoutSession(stripeSession.SessionId);
        
        await _context.SaveChangesAsync(cancellationToken);

        return new PayBannerResponse(stripeSession.CheckoutUrl);
    }
}
