using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Messaging;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Banners.Commands.ApproveBannerRequest;

public sealed class ApproveBannerRequestHandler : IRequestHandler<ApproveBannerRequestCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;
    private readonly IEventBus _eventBus;

    public ApproveBannerRequestHandler(
        IApplicationDbContext context,
        TimeProvider timeProvider,
        ICurrentUserService currentUser,
        IEventBus eventBus)
    {
        _context = context;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
        _eventBus = eventBus;
    }

    public async Task<Guid> Handle(ApproveBannerRequestCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var bannerReq = await _context.BannerRequests
            .FirstOrDefaultAsync(br => br.Id == request.RequestId, cancellationToken)
            ?? throw new NotFoundException(nameof(BannerRequest), request.RequestId);

        if (bannerReq.Status != BannerRequestStatus.Pending || !bannerReq.IsPaid)
            throw new InvalidOperationException("Only paid and pending requests can be approved.");

        if (request.IsActive)
        {
            var isPositionTaken = await _context.Banners.AnyAsync(b => b.SortOrder == request.SortOrder && b.IsActive, cancellationToken);
            if (isPositionTaken) throw new PositionConflictException(request.SortOrder, "Banner");
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        bannerReq.Approve(utcNow);

        string smartCode = Guid.NewGuid().ToString("N")[..8]; 
        
        var smartLinkEntityType = bannerReq.AlbumId.HasValue
            ? SmartLinkType.Album
            : SmartLinkType.Artist;
        var smartLinkEntityId = bannerReq.AlbumId ?? bannerReq.ArtistId;

        var smartLink = SmartLink.Create(
            code: smartCode,
            entityType: smartLinkEntityType,
            entityId: smartLinkEntityId,
            createdByUserId: adminId,
            expiresAt: null,
            utcNow: utcNow
        );
        _context.SmartLinks.Add(smartLink);

        string targetUrl = $"/sl/{smartCode}";

        var activeBanner = Banner.Create(
            title: bannerReq.Title,
            bannerUrl: bannerReq.BannerUrl ?? string.Empty,
            targetUrl: targetUrl, 
            sortOrder: request.SortOrder,
            isActive: request.IsActive,
            utcNow: utcNow,
            artistId: bannerReq.ArtistId,
            startsAtUtc: request.StartsAtUtc
        );

        _context.Banners.Add(activeBanner);
        await _context.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(
            new BannerRequestApprovedEvent(
                bannerReq.SubmittedByUserId,
                bannerReq.ArtistId,
                bannerReq.Title),
            cancellationToken);

        return activeBanner.Id;
    }
}
