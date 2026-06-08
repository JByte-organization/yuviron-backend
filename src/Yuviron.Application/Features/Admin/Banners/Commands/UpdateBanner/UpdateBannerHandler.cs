using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions; 
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Banners.Commands.UpdateBanner;

public sealed class UpdateBannerHandler : IRequestHandler<UpdateBannerCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public UpdateBannerHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        ICurrentUserService currentUser)
    {
        _context = context;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UpdateBannerCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();

        var banner = await _context.Banners
            .FirstOrDefaultAsync(b => b.Id == request.BannerId, cancellationToken);

        if (banner == null)
        {
            throw new NotFoundException(nameof(Banner), request.BannerId);
        }

        if (request.IsActive)
        {
            var isPositionTaken = await _context.Banners
                .AnyAsync(b => b.SortOrder == request.SortOrder && b.Id != request.BannerId && b.IsActive, cancellationToken);

            if (isPositionTaken)
            {
                throw new PositionConflictException(request.SortOrder, "Banner");
            }
        }
        
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        string finalBannerUrl = banner.BannerUrl; 
        
        if (request.BannerFileId.HasValue)
        {
            var bannerClaim = await _context.ClaimFileAsync(
                request.BannerFileId.Value, adminId, "image/", "banners", cancellationToken);
            
            banner.RegisterFileSwapEvents(bannerClaim, banner.BannerUrl);
            finalBannerUrl = bannerClaim.FinalPath;
        }
        
        banner.Update(
            request.Title, 
            finalBannerUrl, 
            request.TargetUrl, 
            request.SortOrder, 
            request.IsActive, 
            utcNow,
            request.ArtistId,
            request.StartsAtUtc);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
