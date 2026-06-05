using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Enums;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Banners.Commands.ApproveBannerRequest;

public sealed class ApproveBannerRequestHandler : IRequestHandler<ApproveBannerRequestCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public ApproveBannerRequestHandler(IApplicationDbContext context, TimeProvider timeProvider, ICurrentUserService currentUser)
    {
        _context = context; _timeProvider = timeProvider; _currentUser = currentUser;
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
        
        var smartLink = SmartLink.Create(
            code: smartCode,
            entityType: SmartLinkType.Album, 
            entityId: bannerReq.AlbumId!.Value,
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
            utcNow: utcNow
        );

        _context.Banners.Add(activeBanner);
        await _context.SaveChangesAsync(cancellationToken);

        return activeBanner.Id;
    }
}