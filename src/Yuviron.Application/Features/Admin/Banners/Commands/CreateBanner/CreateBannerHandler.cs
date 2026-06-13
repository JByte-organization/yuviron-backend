using MediatR;
using Microsoft.EntityFrameworkCore;
using Yuviron.Application.Abstractions;
using Yuviron.Application.Abstractions.Services;
using Yuviron.Application.Extensions;
using Yuviron.Domain.Entities;
using Yuviron.Domain.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Yuviron.Application.Features.Admin.Banners.Commands.CreateBanner;

public sealed class CreateBannerHandler : IRequestHandler<CreateBannerCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserService _currentUser;

    public CreateBannerHandler(
        IApplicationDbContext context, 
        TimeProvider timeProvider,
        ICurrentUserService currentUser)
    {
        _context = context;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateBannerCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUser.UserId ?? throw new UnauthorizedAccessException();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;

        var bannerClaim = await _context.ClaimFileAsync(
            request.BannerFileId, adminId, "image/", "banners", cancellationToken);

        var banner = Banner.Create(
            request.Title,
            bannerClaim.FinalPath, 
            request.TargetUrl,
            request.IsActive,
            utcNow,
            request.ArtistId,
            request.StartsAtUtc,
            request.EndsAtUtc,
            request.TargetCountries,
            request.TargetGenres
        );

        banner.RegisterFileSwapEvents(bannerClaim);

        _context.Banners.Add(banner);
        await _context.SaveChangesAsync(cancellationToken);

        return banner.Id;
    }
}
