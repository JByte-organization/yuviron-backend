using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Extensions; 
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Banners.Commands.UpdateBanner;

public sealed class UpdateBannerHandler : IRequestHandler<UpdateBannerCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public UpdateBannerHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Unit> Handle(UpdateBannerCommand request, CancellationToken cancellationToken)
    {
        var banner = await _context.Banners
                         .FirstOrDefaultAsync(b => b.Id == request.BannerId, cancellationToken)
                     ?? throw new NotFoundException(nameof(Banner), request.BannerId);

        var isPositionTaken = await _context.Banners
            .AnyAsync(b => b.SortOrder == request.SortOrder && b.Id != request.BannerId, cancellationToken);

        if (isPositionTaken)
        {
            throw new PositionConflictException(request.SortOrder, "Banner");
        }
        
        var oldBannerUrl = banner.BannerUrl; 
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        
        var finalBannerUrl = FileStorageExtensions.PredictDestinationPath(request.BannerUrl, "banners");
        
        banner.Update(
            request.Title, 
            finalBannerUrl, 
            request.TargetUrl, 
            request.SortOrder, 
            request.IsActive, 
            utcNow);

        if (!string.IsNullOrWhiteSpace(request.BannerUrl) && request.BannerUrl.StartsWith("temp/"))
        {
            banner.AddDomainEvent(new TempFileNeedsMovingEvent(request.BannerUrl, "banners"));
        }

        if (!string.Equals(oldBannerUrl, finalBannerUrl, StringComparison.OrdinalIgnoreCase) 
            && !string.IsNullOrWhiteSpace(oldBannerUrl))
        {
            banner.AddDomainEvent(new FileNeedsDeletionEvent(oldBannerUrl));
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}