using Yuviron.Application.Abstractions.Data.Contexts;
using Yuviron.Application.Abstractions.Data;
﻿using MediatR;
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
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Yuviron.Application.Features.StudioArtist.Marketing.Commands.SubmitBannerRequest;

public sealed class SubmitBannerRequestHandler : IRequestHandler<SubmitBannerRequestCommand, SubmitBannerResponse>
{
    private readonly IIdentityContext _identityContext;
    private readonly ICatalogContext _catalogContext;
    private readonly IContentContext _contentContext;
    private readonly ISystemContext _systemContext;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;
    private readonly IPaymentService _paymentService;
    private readonly MarketingOptions _options; 

    public SubmitBannerRequestHandler(
        IIdentityContext identityContext, ICatalogContext catalogContext, IContentContext contentContext, ISystemContext systemContext, 
        TimeProvider timeProvider, 
        ICurrentUserService currentUser, 
        IPaymentService paymentService,
        IOptions<MarketingOptions> options) 
    {
        _identityContext = identityContext;
        _catalogContext = catalogContext;
        _contentContext = contentContext;
        _systemContext = systemContext; 
        _timeProvider = timeProvider; 
        _currentUser = currentUser; 
        _paymentService = paymentService;
        _options = options.Value;
    }

    public async Task<SubmitBannerResponse> Handle(SubmitBannerRequestCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var user = await _identityContext.Users.FirstAsync(u => u.Id == userId, cancellationToken);

        var hasPermission = await _catalogContext.ArtistTeamMembers
            .HasManagementAccess(request.ArtistId, userId)
            .AnyAsync(cancellationToken);

        if (!hasPermission) throw new ForbiddenException("No access to manage marketing for this artist.");

        var albumExists = await _catalogContext.AlbumArtists
            .AnyAsync(aa => aa.ArtistId == request.ArtistId && aa.AlbumId == request.AlbumId, cancellationToken);
            
        if (!albumExists) throw new ForbiddenException("The selected album does not belong to this artist.");

        var existingPendingRequest = await _contentContext.BannerRequests
            .AnyAsync(br => br.ArtistId == request.ArtistId && (br.Status == BannerRequestStatus.Pending || br.Status == BannerRequestStatus.AwaitingPayment), cancellationToken);

        if (existingPendingRequest)
            throw new InvalidOperationException("You already have a pending banner request.");

        if (request.DurationDays < 1) throw new InvalidOperationException("Duration must be at least 1 day.");

        var bannerClaim = await _systemContext.ClaimFileAsync(
            request.BannerFileId, userId, "image/", "banners", cancellationToken);

        var bannerRequest = BannerRequest.Create(
            artistId: request.ArtistId, 
            userId: userId, 
            albumId: request.AlbumId,
            title: request.Title, 
            bannerUrl: bannerClaim.FinalPath,
            durationDays: request.DurationDays,
            targetCountries: request.TargetCountries,
            targetGenres: request.TargetGenres,
            utcNow: utcNow);

        bannerRequest.RegisterFileSwapEvents(bannerClaim);
        _contentContext.Add(bannerRequest);

        decimal totalPrice = request.DurationDays * _options.BannerPricePerDay;

        var stripeSession = await _paymentService.CreateBannerCheckoutSessionAsync(
            user, bannerRequest, totalPrice, _options.BannerCurrency, request.SuccessUrl, request.CancelUrl, cancellationToken);

        bannerRequest.SetCheckoutSession(stripeSession.SessionId);
        
        await _identityContext.SaveChangesAsync(cancellationToken);

        return new SubmitBannerResponse(bannerRequest.Id, stripeSession.CheckoutUrl);
    }
}
