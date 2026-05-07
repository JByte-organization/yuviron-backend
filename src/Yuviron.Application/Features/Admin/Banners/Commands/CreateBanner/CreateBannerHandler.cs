using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Events;
using Yuviron.Domain.Exceptions;

namespace Yuviron.Application.Features.Admin.Banners.Commands.CreateBanner;

public sealed class CreateBannerHandler : IRequestHandler<CreateBannerCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;

    public CreateBannerHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider)
    {
        _context = context;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> Handle(CreateBannerCommand request, CancellationToken cancellationToken)
    {
        if (request.IsActive)
        {
            var isPositionTaken = await _context.Banners
                .AnyAsync(b => b.SortOrder == request.SortOrder && b.IsActive, cancellationToken);

            if (isPositionTaken)
            {
                throw new PositionConflictException(request.SortOrder, "Banner");
            }
        }
        
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var finalBannerUrl = FileStorageExtensions.PredictDestinationPath(request.BannerUrl, "banners");

        var banner = Banner.Create(
            request.Title,
            finalBannerUrl, 
            request.TargetUrl,
            request.SortOrder,
            request.IsActive,
            utcNow
        );

        if (!string.IsNullOrWhiteSpace(request.BannerUrl) && request.BannerUrl.StartsWith("temp/"))
        {
            banner.AddDomainEvent(new TempFileNeedsMovingEvent(request.BannerUrl, "banners"));
        }

        _context.Banners.Add(banner);
        
        await _context.SaveChangesAsync(cancellationToken);

        return banner.Id;
    }
}