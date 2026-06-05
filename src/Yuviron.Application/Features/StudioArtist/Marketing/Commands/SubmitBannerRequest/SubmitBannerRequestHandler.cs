using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Payment;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Configuration; 
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.StudioArtist.Marketing.Commands.SubmitBannerRequest;

public sealed class SubmitBannerRequestHandler : IRequestHandler<SubmitBannerRequestCommand, SubmitBannerResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;
    private readonly IPaymentService _paymentService;
    private readonly MarketingOptions _options; 

    public SubmitBannerRequestHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider, 
        ICurrentUserService currentUser, 
        IPaymentService paymentService,
        IOptions<MarketingOptions> options) 
    {
        _context = context; 
        _timeProvider = timeProvider; 
        _currentUser = currentUser; 
        _paymentService = paymentService;
        _options = options.Value;
    }

    public async Task<SubmitBannerResponse> Handle(SubmitBannerRequestCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var user = await _context.Users.FirstAsync(u => u.Id == userId, cancellationToken);

        var hasPermission = await _context.ArtistTeamMembers
            .HasManagementAccess(request.ArtistId, userId)
            .AnyAsync(cancellationToken);

        if (!hasPermission) throw new ForbiddenException("No access to manage marketing for this artist.");

        var albumExists = await _context.AlbumArtists
            .AnyAsync(aa => aa.ArtistId == request.ArtistId && aa.AlbumId == request.AlbumId, cancellationToken);
            
        if (!albumExists) throw new ForbiddenException("The selected album does not belong to this artist.");

        var existingPendingRequest = await _context.BannerRequests
            .AnyAsync(br => br.ArtistId == request.ArtistId && br.Status == BannerRequestStatus.Pending, cancellationToken);

        if (existingPendingRequest)
            throw new InvalidOperationException("You already have a pending banner request.");

        var bannerClaim = await _context.ClaimFileAsync(
            request.BannerFileId, userId, "image/", "banners", cancellationToken);

        var bannerRequest = BannerRequest.Create(
            artistId: request.ArtistId, userId: userId, albumId: request.AlbumId,
            title: request.Title, bannerUrl: bannerClaim.FinalPath, utcNow: utcNow);

        bannerRequest.RegisterFileSwapEvents(bannerClaim);
        _context.BannerRequests.Add(bannerRequest);

        var stripeSession = await _paymentService.CreateBannerCheckoutSessionAsync(
            user, bannerRequest, _options.BannerPrice, _options.BannerCurrency, request.SuccessUrl, request.CancelUrl, cancellationToken);

        bannerRequest.SetCheckoutSession(stripeSession.SessionId);
        
        await _context.SaveChangesAsync(cancellationToken);

        return new SubmitBannerResponse(bannerRequest.Id, stripeSession.CheckoutUrl);
    }
}